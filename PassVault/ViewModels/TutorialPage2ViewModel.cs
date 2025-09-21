using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Interfaces;
using PassVault.Services;
using PassVault.Views;
using System.Windows.Input;

namespace PassVault.ViewModels
{
    public partial class TutorialPage2ViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string advancedSecurityTitle;

        [ObservableProperty]
        private string militaryGradeProtection;

        [ObservableProperty]
        private string aesEncryption;

        [ObservableProperty]
        private string militaryStandard;

        [ObservableProperty]
        private string localStorage;

        [ObservableProperty]
        private string onDeviceOnly;

        [ObservableProperty]
        private string zeroKnowledge;

        [ObservableProperty]
        private string weCantSeeYourData;

        [ObservableProperty]
        private string progressText;

        [ObservableProperty]
        private string nextButtonText;

        public ICommand NextPageCommand { get; }

        public TutorialPage2ViewModel(ILocalizationService localizationService)
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
            Title = L.Text("tutorial.security_title");
            Description = L.Text("tutorial.security_description");
            AdvancedSecurityTitle = L.Text("tutorial.advanced_security_title");
            MilitaryGradeProtection = L.Text("tutorial.military_grade_protection");
            AesEncryption = L.Text("tutorial.aes_encryption");
            MilitaryStandard = L.Text("tutorial.military_standard");
            LocalStorage = L.Text("tutorial.local_storage");
            OnDeviceOnly = L.Text("tutorial.on_device_only");
            ZeroKnowledge = L.Text("tutorial.zero_knowledge");
            WeCantSeeYourData = L.Text("tutorial.we_cant_see_your_data");
            ProgressText = L.Text("tutorial.progress_2_of_5");
            NextButtonText = L.Text("common.next");
        }

        private async void OnNextPageClicked()
        {
            await Shell.Current.GoToAsync(nameof(TutorialPage3));
        }
    }
}