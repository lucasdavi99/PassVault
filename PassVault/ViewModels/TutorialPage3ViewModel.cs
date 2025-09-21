using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Interfaces;
using PassVault.Services;
using PassVault.Views;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using System.Runtime.InteropServices;
using System.Windows.Input;


namespace PassVault.ViewModels
{
    public partial class TutorialPage3ViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string fingerprint;

        [ObservableProperty]
        private string unlockWithFinger;

        [ObservableProperty]
        private string faceId;

        [ObservableProperty]
        private string unlockWithFace;

        [ObservableProperty]
        private string pinOrPattern;

        [ObservableProperty]
        private string defaultDevicePassword;

        [ObservableProperty]
        private string authorizeAccess;

        [ObservableProperty]
        private string tapToSetup;

        [ObservableProperty]
        private string requiredStepMessage;

        [ObservableProperty]
        private string progressText;

        [ObservableProperty]
        private string configureAbove;

        public ICommand NextPageCommand { get; }

        public TutorialPage3ViewModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += OnLanguageChanged;
            UpdateLocalizedTexts();
            NextPageCommand = new RelayCommand(OnNextPageClicked);
        }

        private void OnLanguageChanged(object sender, System.EventArgs e)
        {
            UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            Title = L.Text("tutorial.app_password_title");
            Description = L.Text("tutorial.app_password_description");
            Fingerprint = L.Text("tutorial.fingerprint");
            UnlockWithFinger = L.Text("tutorial.unlock_with_finger");
            FaceId = L.Text("tutorial.face_id");
            UnlockWithFace = L.Text("tutorial.unlock_with_face");
            PinOrPattern = L.Text("tutorial.pin_or_pattern");
            DefaultDevicePassword = L.Text("tutorial.default_device_password");
            AuthorizeAccess = L.Text("tutorial.authorize_access");
            TapToSetup = L.Text("tutorial.tap_to_setup");
            RequiredStepMessage = L.Text("tutorial.required_step_message");
            ProgressText = L.Text("tutorial.progress_3_of_5");
            ConfigureAbove = L.Text("tutorial.configure_above");
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
                await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.auth_failed"), L.Text("common.ok"));
            }
        }

        public async Task<bool> AuthenticateUserAsync()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    await Shell.Current.DisplayAlert("Simulation", "Simulated authentication on Windows.", L.Text("common.ok"));
                    return true;
                }

                var config = new AuthenticationRequestConfiguration(
                    L.Text("messages.auth_needed_title"),
                    L.Text("messages.auth_needed_message"))
                {
                    AllowAlternativeAuthentication = true,
                };

                var authResult = await CrossFingerprint.Current.AuthenticateAsync(config);

                if (authResult.Status == FingerprintAuthenticationResultStatus.NotAvailable)
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.no_password_configured"), L.Text("common.ok"));
                    return false;
                }

                if (authResult.Authenticated)
                {
                    await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("messages.auth_success"), L.Text("common.ok"));
                    return true;
                }
                else
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.auth_failed"), L.Text("common.ok"));
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
                return false;
            }
        }
    }
}