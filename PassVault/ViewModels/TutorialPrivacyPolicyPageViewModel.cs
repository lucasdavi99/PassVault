using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using PassVault.Interfaces;
using PassVault.Services;
using PassVault.Views;
using System.Threading.Tasks;

namespace PassVault.ViewModels
{
    public partial class TutorialPrivacyPolicyPageViewModel : ObservableObject
    {
        private const string PrivacyPolicyAcceptedKey = "PrivacyPolicyAccepted";
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string intro;

        [ObservableProperty]
        private string acceptanceTitle;

        [ObservableProperty]
        private string acceptanceDescription;

        [ObservableProperty]
        private string responsibleUseTitle;

        [ObservableProperty]
        private string responsibleUseDescription;

        [ObservableProperty]
        private string securityTitle;

        [ObservableProperty]
        private string securityDescription;

        [ObservableProperty]
        private string liabilityTitle;

        [ObservableProperty]
        private string liabilityDescription;

        [ObservableProperty]
        private string updatesTitle;

        [ObservableProperty]
        private string updatesDescription;

        [ObservableProperty]
        private string acknowledgementText;

        [ObservableProperty]
        private bool isPolicyAccepted;

        [ObservableProperty]
        private string progressText;

        [ObservableProperty]
        private string nextButtonText;

        public IAsyncRelayCommand ContinueCommand { get; }

        public TutorialPrivacyPolicyPageViewModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += OnLanguageChanged;
            UpdateLocalizedTexts();
            IsPolicyAccepted = Preferences.Get(PrivacyPolicyAcceptedKey, false);
            ContinueCommand = new AsyncRelayCommand(OnContinueAsync);
        }

        private void OnLanguageChanged(object sender, System.EventArgs e)
        {
            UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            Title = L.Text("tutorial.privacy_policy_title");
            Intro = L.Text("tutorial.privacy_policy_intro");
            AcceptanceTitle = L.Text("tutorial.privacy_policy_acceptance_title");
            AcceptanceDescription = L.Text("tutorial.privacy_policy_acceptance_description");
            ResponsibleUseTitle = L.Text("tutorial.privacy_policy_responsible_use_title");
            ResponsibleUseDescription = L.Text("tutorial.privacy_policy_responsible_use_description");
            SecurityTitle = L.Text("tutorial.privacy_policy_security_title");
            SecurityDescription = L.Text("tutorial.privacy_policy_security_description");
            LiabilityTitle = L.Text("tutorial.privacy_policy_liability_title");
            LiabilityDescription = L.Text("tutorial.privacy_policy_liability_description");
            UpdatesTitle = L.Text("tutorial.privacy_policy_updates_title");
            UpdatesDescription = L.Text("tutorial.privacy_policy_updates_description");
            AcknowledgementText = L.Text("tutorial.privacy_policy_acknowledgement");
            ProgressText = L.Text("tutorial.progress_5_of_6");
            NextButtonText = L.Text("common.continue_button");
        }

        private async Task OnContinueAsync()
        {
            if (!IsPolicyAccepted)
            {
                await Shell.Current.DisplayAlert(L.Text("common.warning"),
                    L.Text("tutorial.privacy_policy_acceptance_required"),
                    L.Text("common.ok"));
                return;
            }

            Preferences.Set(PrivacyPolicyAcceptedKey, true);
            await Shell.Current.GoToAsync(nameof(TutorialPage5));
        }
    }
}
