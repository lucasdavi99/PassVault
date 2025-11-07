using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Interfaces;
using PassVault.Services;
using PassVault.Services.Security;
using System.Windows.Input;

namespace PassVault.ViewModels
{
    public partial class LockScreenViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;
        private readonly IAuthenticationService _authenticationService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string buttonText;

        public ICommand NextPageCommand { get; }

        public LockScreenViewModel(ILocalizationService localizationService, IAuthenticationService authenticationService)
        {
            _localizationService = localizationService;
            _authenticationService = authenticationService;
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
                var request = new AppAuthenticationRequest
                {
                    Title = L.Text("lockscreen.auth.title"),
                    Message = L.Text("lockscreen.auth.message"),
                    AllowAlternativeAuthentication = true,
                };

                var result = await _authenticationService.AuthenticateAsync(request);

                if (result.Status == AppAuthenticationStatus.NotAvailable)
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.no_password_configured"), L.Text("common.ok"));
                    return false;
                }

                if (!result.IsSuccessful)
                {
                    var message = !string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? string.Format(L.Text("messages.auth_error"), result.ErrorMessage)
                        : L.Text("messages.auth_failed");

                    await Shell.Current.DisplayAlert(L.Text("common.error"), message, L.Text("common.ok"));
                }

                return result.IsSuccessful;
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
