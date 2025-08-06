using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Views;

namespace PassVault.ViewModels
{
    public partial class NewAccountPageViewModel : ObservableValidator, IQueryAttributable
    {
        private readonly AccountDatabase _database;

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
        private int? folderId;

        [ObservableProperty]
        private Color _selectedColor = Colors.Purple;

        [ObservableProperty]
        private bool _isColorPickerVisible = false;

        [ObservableProperty]
        private string _selectedColorHex = Colors.Purple.ToHex();

        //Campos selecionados.

        [ObservableProperty] private bool isUsernameVisible = true;
        [ObservableProperty] private bool isEmailVisible = true;

        public NewAccountPageViewModel(AccountDatabase database)
        {
            _database = database;

            WeakReferenceMessenger.Default.Register<PasswordGeneratedMessage>(this, (r, m) => { Password = m.Value; });
        }

        [RelayCommand]
        private async Task SaveAccountAsync()
        {
            try
            {
                ValidateAllProperties();

                if (HasErrors)
                {
                    await Shell.Current.DisplayAlert("Erro", "Preencha os campos corretamente", "OK");
                    return;
                }

                var account = new Account
                {
                    Title = Title,
                    Username = Username,
                    Email = Email,
                    Password = Password,
                    Created = DateTime.Now,
                    Color = SelectedColor.ToHex(),
                    FolderId = FolderId
                };

                await _database.SaveAccountAsync(account);
                await Shell.Current.DisplayAlert("Sucesso", "Conta salva com sucesso", "OK");
                WeakReferenceMessenger.Default.Send(new AccountSavedMessage(true));
                await Shell.Current.Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task GoToGenerator() => await Shell.Current.GoToAsync(nameof(PasswordGenerator));

        [RelayCommand]
        private void ToggleColorPicker()
        {
            IsColorPickerVisible = !IsColorPickerVisible;
        }

        [RelayCommand]
        private void CloseColorPicker()
        {
            IsColorPickerVisible = false;
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

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
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

            if (query.ContainsKey("folderId") && int.TryParse(query["folderId"]?.ToString(), out int folderId))
            {
                FolderId = folderId;
            }
        }
    }
}
