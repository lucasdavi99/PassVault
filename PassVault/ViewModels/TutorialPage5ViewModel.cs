using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Interfaces;
using PassVault.Models;
using PassVault.Services;
using System.Windows.Input;

namespace PassVault.ViewModels
{
    public partial class TutorialPage5ViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string congratulations;

        [ObservableProperty]
        private string readyToStartMessage;

        [ObservableProperty]
        private string createAndOrganize;

        [ObservableProperty]
        private string generateSecurePasswords;

        [ObservableProperty]
        private string searchAndAccess;

        [ObservableProperty]
        private string backupData;

        [ObservableProperty]
        private string welcome;

        [ObservableProperty]
        private string youAreNowAUser;

        [ObservableProperty]
        private string progressText;

        [ObservableProperty]
        private string getStartedButton;

        [ObservableProperty]
        private string accessPassVault;

        public ICommand NextPageCommand { get; }

        public TutorialPage5ViewModel(ILocalizationService localizationService)
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
            Title = L.Text("tutorial.ready_title");
            Congratulations = L.Text("tutorial.congratulations");
            ReadyToStartMessage = L.Text("tutorial.ready_to_start_message");
            CreateAndOrganize = L.Text("tutorial.create_and_organize");
            GenerateSecurePasswords = L.Text("tutorial.generate_secure_passwords");
            SearchAndAccess = L.Text("tutorial.search_and_access");
            BackupData = L.Text("tutorial.backup_data");
            Welcome = L.Text("tutorial.welcome");
            YouAreNowAUser = L.Text("tutorial.you_are_now_a_user");
            ProgressText = L.Text("tutorial.progress_5_of_5");
            GetStartedButton = L.Text("tutorial.get_started_button");
            AccessPassVault = L.Text("tutorial.access_passvault");
        }

        private async void OnNextPageClicked()
        {
            Preferences.Set("IsNewUser", false);
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}