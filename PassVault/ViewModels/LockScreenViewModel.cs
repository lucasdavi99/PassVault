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
                await Shell.Current.DisplayAlert("Simulação", "Autenticação simulada no Windows.", "OK");
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

                var config = new AuthenticationRequestConfiguration("Autenticação necessária", "Para acessar o aplicativo, autorize o uso da senha padrão do seu smartphone.");
                var authResult = await CrossFingerprint.Current.AuthenticateAsync(config);

                if (authResult.Authenticated)
                {
                    return true;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erro", "Falha na autenticação. Tente novamente.", "OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
                return false;
            }
        }
    }
}
