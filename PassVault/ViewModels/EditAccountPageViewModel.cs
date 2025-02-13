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
using System.Runtime.InteropServices;


namespace PassVault.ViewModels
{
    public partial class EditAccountPageViewModel : ObservableValidator, IQueryAttributable
    {
        private readonly AccountDatabase _database;
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
        private bool _isColorPickerVisible = false;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private bool _isPasswordVisible = true;

        public EditAccountPageViewModel(AccountDatabase database)
        {
            _database = database;
            IsEditing = false;
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
            OnPropertyChanged(nameof(SelectedColor));
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if(query.ContainsKey("accountId") && int.TryParse(query["accountId"]?.ToString(), out int accountId))
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
        }
    }
}
