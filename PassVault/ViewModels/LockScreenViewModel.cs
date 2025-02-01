using CommunityToolkit.Mvvm.Input;
using Plugin.Fingerprint.Abstractions;
using Plugin.Fingerprint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PassVault.Views;

namespace PassVault.ViewModels
{
    internal class LockScreenViewModel
    {
        public string Title => "Bem vindo!";
        public ICommand NextPageCommand { get; }

        public LockScreenViewModel()
        {
            NextPageCommand = new RelayCommand(OnNextPageClicked);
        }

        private async void OnNextPageClicked()
        {
            var authResult = await AuthenticateUserAsync();

            if (authResult)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
        }

        private async Task<bool> AuthenticateUserAsync()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return true;
                }

                var config = new AuthenticationRequestConfiguration(
                    "Autenticação necessária",
                    "Para acessar o aplicativo, autorize o uso da senha padrão do seu smartphone.")
                {
                    AllowAlternativeAuthentication = true,
                };
                var authResult = await CrossFingerprint.Current.AuthenticateAsync(config);

                return authResult.Authenticated;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
                return false;
            }
        }
    }
}
