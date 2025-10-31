using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Interfaces;
using PassVault.Services;
using PassVault.Views;
using System.Windows.Input;

namespace PassVault.ViewModels
{
    public partial class TutorialPage1ViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string pageTitle;

        [ObservableProperty]
        private string pageSubtitle;

        [ObservableProperty]
        private string createAndOrganizeText;

        [ObservableProperty]
        private string secureStorageText;

        [ObservableProperty]
        private string passwordGeneratorText;

        [ObservableProperty]
        private string progressText;

        [ObservableProperty]
        private string nextButtonText;

        public ICommand NextPageCommand { get; }

        public TutorialPage1ViewModel(ILocalizationService localizationService)
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
            Title = L.Text("tutorial.welcome_title");
            Description = L.Text("tutorial.welcome_description");
            PageTitle = L.Text("tutorial.page_title");
            PageSubtitle = L.Text("tutorial.page_subtitle");
            CreateAndOrganizeText = L.Text("tutorial.create_and_organize_text");
            SecureStorageText = L.Text("tutorial.secure_storage_text");
            PasswordGeneratorText = L.Text("tutorial.password_generator_text");
            ProgressText = L.Text("tutorial.progress_text_1_of_6");
            NextButtonText = L.Text("common.next");
        }

        private async void OnNextPageClicked()
        {
            await Shell.Current.GoToAsync(nameof(TutorialPage2));
        }
    }
}
