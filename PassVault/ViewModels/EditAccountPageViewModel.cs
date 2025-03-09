using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Views;
using Plugin.Fingerprint.Abstractions;
using Plugin.Fingerprint;
using System.ComponentModel.DataAnnotations;

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
        private string _selectedFolderName = "Selecione a Pasta";

        //Campos selecionados.

        [ObservableProperty] private bool isUsernameVisible = true;
        [ObservableProperty] private bool isEmailVisible = true;

        public EditAccountPageViewModel(AccountDatabase database, FolderDatabase folderDatabase)
        {
            _database = database;
            IsEditing = false;

            _folderDatabase = folderDatabase;
        }

        [RelayCommand]
        private async Task SelectFolderAsync()
        {
            Folders = await _folderDatabase.GetFoldersAsync();

            if (Folders == null || Folders.Count == 0)
            {
                await Shell.Current.DisplayAlert("Atenção", "Nenhuma pasta encontrada.", "OK");
                return;
            }

            var folderNames = Folders.Select(f => f.Title).ToList();
            folderNames.Insert(0, "Sem Pasta");

            string chosenOption = await Shell.Current.DisplayActionSheet( "Selecione uma pasta", "Cancelar", null, folderNames.ToArray());

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
                    SelectedFolderName = chosenOption;
                    _currentAccount.FolderId = chosenOption == "Sem Pasta" ? null : Folders.First(f => f.Title == chosenOption).Id;
                }
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
                else
                {
                    _currentAccount.Title = Title;
                    _currentAccount.Username = Username;
                    _currentAccount.Email = Email;
                    _currentAccount.Password = Password;
                    _currentAccount.Color = SelectedColor.ToHex();
                };

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
                //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                //{
                //    return true;
                //}

                var config = new AuthenticationRequestConfiguration("Autenticação necessária", "Desbloqueie o Dispositivo.")
                {
                    AllowAlternativeAuthentication = true,
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
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
                return false;
            }
        }

        partial void OnSelectedColorChanged(Color value)
        {
            // Força a atualização da interface
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
                _currentAccount = await _database.GetAccountAsync(accountId);
                if (_currentAccount != null)
                {
                    Title = _currentAccount.Title;
                    Username = _currentAccount.Username;
                    Email = _currentAccount.Email;
                    Password = _currentAccount.Password;
                    SelectedColor = Color.FromArgb(_currentAccount.Color);
                }
            }

            //Query dos campos selecionados para visibilidade
            if (query.ContainsKey("selectedFields"))
            {
                if (query["selectedFields"] is Dictionary<string, bool> fields)
                {
                    if (fields.TryGetValue("Username", out bool usernameVisible))
                        IsUsernameVisible = usernameVisible;

                    if (fields.TryGetValue("Email", out bool emailVisible))
                        IsEmailVisible = emailVisible;
                }
            }
        }
    }
}