using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Interfaces;
using PassVault.Services;
using PassVault.Views;
using System.Windows.Input;

namespace PassVault.ViewModels
{
    public partial class TutorialPage4ViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string privacyCommitmentTitle;

        [ObservableProperty]
        private string yourDataIsYours;

        [ObservableProperty]
        private string local;

        [ObservableProperty]
        private string storedOnDevice;

        [ObservableProperty]
        private string noCloud;

        [ObservableProperty]
        private string noDataToServer;

        [ObservableProperty]
        private string noTracking;

        [ObservableProperty]
        private string noPersonalInfo;

        [ObservableProperty]
        private string transparent;

        [ObservableProperty]
        private string auditableCode;

        [ObservableProperty]
        private string private100;

        [ObservableProperty]
        private string privacyFocused;

        [ObservableProperty]
        private string progressText;

        [ObservableProperty]
        private string nextButtonText;

        public ICommand NextPageCommand { get; }

        public TutorialPage4ViewModel(ILocalizationService localizationService)
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
            Title = L.Text("tutorial.info_title");
            Description = L.Text("tutorial.info_description");
            PrivacyCommitmentTitle = L.Text("tutorial.privacy_commitment_title");
            YourDataIsYours = L.Text("tutorial.your_data_is_yours");
            Local = L.Text("tutorial.local");
            StoredOnDevice = L.Text("tutorial.stored_on_device");
            NoCloud = L.Text("tutorial.no_cloud");
            NoDataToServer = L.Text("tutorial.no_data_to_server");
            NoTracking = L.Text("tutorial.no_tracking");
            NoPersonalInfo = L.Text("tutorial.no_personal_info");
            Transparent = L.Text("tutorial.transparent");
            AuditableCode = L.Text("tutorial.auditable_code");
            Private100 = L.Text("tutorial.private_100");
            PrivacyFocused = L.Text("tutorial.privacy_focused");
            ProgressText = L.Text("tutorial.progress_4_of_6");
            NextButtonText = L.Text("common.next");
        }

        private async void OnNextPageClicked()
        {
            await Shell.Current.GoToAsync(nameof(TutorialPrivacyPolicyPage));
        }
    }
}
