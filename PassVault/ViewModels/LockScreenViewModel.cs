using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Interfaces;
using PassVault.Services;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace PassVault.ViewModels
{
    public partial class LockScreenViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string buttonText;

        public ICommand NextPageCommand { get; }

        public LockScreenViewModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += OnLanguageChanged;
            NextPageCommand = new RelayCommand(OnNextPageClicked);
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(UpdateLocalizedTexts);
        }

        private void UpdateLocalizedTexts()
        {
            Title = L.Text("lockscreen.title");
            ButtonText = L.Text("lockscreen.button_text");
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
                    L.Text("lockscreen.auth.title"), L.Text("lockscreen.auth.message"))
                {
                    AllowAlternativeAuthentication = true,
                };
                var authResult = await CrossFingerprint.Current.AuthenticateAsync(config);

                return authResult.Authenticated;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
                return false;
            }
        }

        ~LockScreenViewModel()
        {
            if (_localizationService != null)
                _localizationService.LanguageChanged -= OnLanguageChanged;
        }
    }
}