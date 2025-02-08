using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Maui.ColorPicker;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.ViewModels
{
    public partial class NewAccountPageViewModel : ObservableValidator
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

        public NewAccountPageViewModel(AccountDatabase database)
        {
            _database = database;
        }

        [RelayCommand]
        private async Task SaveAccountAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Erro", "Preencha os campos obrigatórios", "OK");
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
        private static async Task Close() => await Shell.Current.GoToAsync("..");

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
            OnPropertyChanged(nameof(SelectedColor));
        }
    }
}
