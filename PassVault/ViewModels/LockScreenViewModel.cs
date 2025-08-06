using System.Runtime.InteropServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

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
                    "Autenticação necessária", "Desbloqueie o Dispositivo.")
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
