﻿using CommunityToolkit.Mvvm.Input;
using PassVault.Views;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using System.Runtime.InteropServices;
using System.Windows.Input;


namespace PassVault.ViewModels
{
    class TutorialPage3ViewModel
    {
        public string Title => "Senha do aplicativo";
        public string Description => "Para acessar e utilizar o aplicativo, autorize o uso da senha padrão do seu smartphone clicando no botão abaixo.";
        public ICommand NextPageCommand { get; }

        public TutorialPage3ViewModel()
        {
            NextPageCommand = new RelayCommand(OnNextPageClicked);
        }

        private async void OnNextPageClicked()
        {
            var authResult = await AuthenticateUserAsync();

            if (authResult)
            {
                await Shell.Current.GoToAsync(nameof(TutorialPage4));
            }
            else
            {
                await Shell.Current.DisplayAlert("Erro", "Falha na autenticação. Tente novamente.", "OK");
            }
        }

        public async Task<bool> AuthenticateUserAsync()
        {
            try
            {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    await Shell.Current.DisplayAlert("Simulação", "Autenticação simulada no Windows.", "OK");
                    return true;
                }

                var config = new AuthenticationRequestConfiguration(
                    "Autenticação necessária",
                    "Para acessar o aplicativo, autorize o uso da senha padrão do seu smartphone.")
                {
                    AllowAlternativeAuthentication = true,
                };
                
                var authResult = await CrossFingerprint.Current.AuthenticateAsync(config);
                
                if (authResult.Authenticated)
                {
                    await Shell.Current.DisplayAlert("Sucesso", "Autenticação realizada com sucesso!", "OK");
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
