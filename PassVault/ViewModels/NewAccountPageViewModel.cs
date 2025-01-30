using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Models;
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
                };

                await _database.SaveAccountAsync(account);
                await Shell.Current.DisplayAlert("Sucesso", "Conta salva com sucesso", "OK");
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task GoToGenerator() => await Shell.Current.GoToAsync("///PasswordGenerator");

        [RelayCommand]
        private async Task Close() => await Shell.Current.GoToAsync("///MainPage");


    }
}
