using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Views;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace PassVault.ViewModels
{
    public partial class EditAccountPageViewModel : ObservableValidator, IQueryAttributable
    {
        private readonly AccountDatabase _database;
        private readonly FolderDatabase _folderDatabase;
        private Account _currentAccount;

        [ObservableProperty]
        private int _accountId;

        [ObservableProperty]
        [Required(ErrorMessage = "Título é obrigatório")]
        private string _title;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        private string _email;

        [ObservableProperty]
        [Required(ErrorMessage = "Senha é obrigatória")]
        private string _password;

        [ObservableProperty]
        private Color _selectedColor = Colors.Purple;

        [ObservableProperty]
        private string _selectedColorHex = Colors.Purple.ToHex();

        [ObservableProperty]
        private bool _isColorPickerVisible = false;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private bool _isPasswordVisible = true;

        [ObservableProperty]
        private List<Folder> _folders = new();

        [ObservableProperty]
        private string _selectedFolderName = "Selecionar Pasta";

        // Campos selecionados
        [ObservableProperty]
        private bool isUsernameVisible = true;

        [ObservableProperty]
        private bool isEmailVisible = true;

        // Novas propriedades para subpastas
        [ObservableProperty]
        private Folder _selectedParentFolder;

        [ObservableProperty]
        private List<Folder> _subFolders = new();

        [ObservableProperty]
        private string _selectedSubFolderName = "Selecionar Subpasta";

        [ObservableProperty]
        private bool _hasSubFolderButtonVisible = false;

        public EditAccountPageViewModel(AccountDatabase database, FolderDatabase folderDatabase)
        {
            _database = database;
            IsEditing = false;
            _folderDatabase = folderDatabase;

            WeakReferenceMessenger.Default.Register<PasswordGeneratedMessage>(this, async (r, m) =>
            {
                await Shell.Current.DisplayAlert("Sucesso", "Nova senha aplicada", "OK");
                Password = m.Value;
            });
        }

        [RelayCommand]
        private async Task SelectFolderAsync()
        {
            Folders = await _folderDatabase.GetRootFoldersAsync(); // Carregar apenas pastas raiz

            if (Folders == null || Folders.Count == 0)
            {
                await Shell.Current.DisplayAlert("Atenção", "Nenhuma pasta encontrada.", "OK");
                return;
            }

            var sortedFolders = Folders
               .OrderBy(f => f.Title, StringComparer.OrdinalIgnoreCase)
               .ToList();
            Folders = sortedFolders;

            var folderNames = Folders.Select(f => f.Title).ToList();
            folderNames.Insert(0, "Sem Pasta");

            string chosenOption = await Shell.Current.DisplayActionSheet("Selecione uma pasta", "Cancelar", null, folderNames.ToArray());

            if (chosenOption != null && chosenOption != "Cancelar")
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Confirmação",
                    $"Tem certeza que deseja mover esta conta para {chosenOption}?",
                    "Sim",
                    "Cancelar"
                );

                if (confirm)
                {
                    if (chosenOption == "Sem Pasta")
                    {
                        // Limpar seleções
                        SelectedFolderName = "Sem Pasta";
                        SelectedParentFolder = null;
                        SelectedSubFolderName = "Selecionar Subpasta";
                        HasSubFolderButtonVisible = false;
                        SubFolders.Clear();
                        _currentAccount.FolderId = null;
                    }
                    else
                    {
                        // Pasta selecionada
                        SelectedFolderName = chosenOption;
                        SelectedParentFolder = Folders.First(f => f.Title == chosenOption);
                        _currentAccount.FolderId = SelectedParentFolder.Id;

                        // Resetar subpasta
                        SelectedSubFolderName = "Selecionar Subpasta";

                        // Carregar subpastas e mostrar botão
                        await LoadSubFoldersAsync();
                    }
                }
            }
        }

        [RelayCommand]
        private async Task SelectSubFolderAsync()
        {
            if (SelectedParentFolder == null || SubFolders == null || SubFolders.Count == 0)
            {
                await Shell.Current.DisplayAlert("Atenção", "Nenhuma subpasta encontrada nesta pasta.", "OK");
                return;
            }

            var sortedSubFolders = SubFolders
                .OrderBy(f => f.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var subFolderNames = sortedSubFolders.Select(f => f.Title).ToList();
            subFolderNames.Insert(0, "Manter na Pasta Principal");

            string chosenOption = await Shell.Current.DisplayActionSheet("Selecione uma subpasta", "Cancelar", null, subFolderNames.ToArray());

            if (chosenOption != null && chosenOption != "Cancelar")
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Confirmação",
                    $"Tem certeza que deseja mover esta conta para a subpasta {chosenOption}?",
                    "Sim",
                    "Cancelar"
                );

                if (confirm)
                {
                    if (chosenOption == "Manter na Pasta Principal")
                    {
                        SelectedSubFolderName = "Pasta Principal";
                        _currentAccount.FolderId = SelectedParentFolder.Id;
                    }
                    else
                    {
                        SelectedSubFolderName = chosenOption;
                        var selectedSubFolder = sortedSubFolders.First(f => f.Title == chosenOption);
                        _currentAccount.FolderId = selectedSubFolder.Id;
                    }
                }
            }
        }

        private async Task LoadSubFoldersAsync()
        {
            if (SelectedParentFolder == null)
            {
                HasSubFolderButtonVisible = false;
                return;
            }

            try
            {
                SubFolders = await _folderDatabase.GetSubFoldersAsync(SelectedParentFolder.Id);
                HasSubFolderButtonVisible = SubFolders != null && SubFolders.Count > 0;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar subpastas: {ex.Message}", "OK");
                HasSubFolderButtonVisible = false;
            }
        }

        [RelayCommand]
        private async Task SaveEditAccountAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Erro", "Preencha os campos obrigatórios", "OK");
                    return;
                }

                _currentAccount.Title = Title;
                _currentAccount.Username = Username;
                _currentAccount.Email = Email;
                _currentAccount.Password = Password;
                _currentAccount.Color = SelectedColor.ToHex();

                await _database.SaveAccountAsync(_currentAccount);
                await Shell.Current.DisplayAlert("Sucesso", "Conta atualizada com sucesso", "OK");
                WeakReferenceMessenger.Default.Send(new AccountSavedMessage(true));
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task GoToGenerator() => await Shell.Current.GoToAsync(nameof(PasswordGenerator));

        [RelayCommand]
        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        [RelayCommand]
        private void ToggleColorPicker() => IsColorPickerVisible = !IsColorPickerVisible;

        [RelayCommand]
        private void CloseColorPicker() => IsColorPickerVisible = false;

        [RelayCommand]
        private async Task<bool> ToggleEditMode()
        {
            try
            {
                var config = new AuthenticationRequestConfiguration("Autenticação necessária", "Desbloqueie o Dispositivo.")
                {
                    AllowAlternativeAuthentication = true,
                    CancelTitle = "Cancelar",
                    FallbackTitle = "Use Senha"
                };

                var authResult = await CrossFingerprint.Current.AuthenticateAsync(config);

                if (authResult.Authenticated)
                {
                    IsEditing = !IsEditing;
                    return true;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erro", "Falha na autenticação", "OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro na autenticação: {ex.Message}", "OK");
                return false;
            }
        }

        partial void OnSelectedColorChanged(Color value)
        {
            SelectedColorHex = value.ToHex();
            OnPropertyChanged(nameof(SelectedColor));
        }

        partial void OnSelectedColorHexChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && (value.Length == 7 || value.Length == 9))
            {
                var newColor = Color.FromArgb(value);
                if (newColor != SelectedColor)
                {
                    SelectedColor = newColor;
                }
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("accountId") && int.TryParse(query["accountId"]?.ToString(), out int accountId))
            {
                AccountId = accountId;
                _currentAccount = await _database.GetAccountAsync(accountId);

                if (_currentAccount != null)
                {
                    Title = _currentAccount.Title;
                    Username = _currentAccount.Username ?? string.Empty;
                    Email = _currentAccount.Email ?? string.Empty;
                    Password = _currentAccount.Password;
                    SelectedColor = Color.FromArgb(_currentAccount.Color);

                    // Configurar pasta/subpasta baseado no FolderId
                    await ConfigureFolderDisplayAsync();
                }

                if (query.ContainsKey("selectedFields") && query["selectedFields"] is Dictionary<string, bool> selectedFields)
                {
                    IsUsernameVisible = selectedFields.GetValueOrDefault("Username", true);
                    IsEmailVisible = selectedFields.GetValueOrDefault("Email", true);
                }
            }
        }

        private async Task ConfigureFolderDisplayAsync()
        {
            if (_currentAccount.FolderId == null)
            {
                SelectedFolderName = "Sem Pasta";
                SelectedParentFolder = null;
                SelectedSubFolderName = "Selecionar Subpasta";
                HasSubFolderButtonVisible = false;
                return;
            }

            try
            {
                var currentFolder = await _folderDatabase.GetFolderAsync(_currentAccount.FolderId.Value);

                if (currentFolder == null)
                {
                    SelectedFolderName = "Pasta não encontrada";
                    return;
                }

                if (currentFolder.ParentFolderId == null)
                {
                    // É uma pasta raiz
                    SelectedFolderName = currentFolder.Title;
                    SelectedParentFolder = currentFolder;
                    SelectedSubFolderName = "Pasta Principal";
                    await LoadSubFoldersAsync();
                }
                else
                {
                    // É uma subpasta
                    var parentFolder = await _folderDatabase.GetFolderAsync(currentFolder.ParentFolderId.Value);
                    SelectedFolderName = parentFolder?.Title ?? "Pasta não encontrada";
                    SelectedParentFolder = parentFolder;
                    SelectedSubFolderName = currentFolder.Title;
                    await LoadSubFoldersAsync();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar informações da pasta: {ex.Message}", "OK");
            }
        }
    }
}