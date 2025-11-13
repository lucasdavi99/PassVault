using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrionVault.Interfaces;
using OrionVault.Services;
using OrionVault.Views;

namespace OrionVault.ViewModels
{
    public partial class FieldsSelectionViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private bool isUsernameChecked = true;

        [ObservableProperty]
        private bool isEmailChecked = true;

        [ObservableProperty]
        private int folderId;

        // Localized Properties
        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string pageSubtitle;
        [ObservableProperty] private string requiredFieldsTitle;
        [ObservableProperty] private string requiredFieldsMessage;
        [ObservableProperty] private string usernameTitle;
        [ObservableProperty] private string usernameDescription;
        [ObservableProperty] private string emailTitle;
        [ObservableProperty] private string emailDescription;
        [ObservableProperty] private string previewTitle;
        [ObservableProperty] private string previewSubtitle;
        [ObservableProperty] private string previewAccountTitle;
        [ObservableProperty] private string previewUsername;
        [ObservableProperty] private string previewEmail;
        [ObservableProperty] private string previewPassword;
        [ObservableProperty] private string continueButtonText;

        public FieldsSelectionViewModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = L.Text("fields_selection.title");
            PageSubtitle = L.Text("fields_selection.subtitle");
            RequiredFieldsTitle = L.Text("fields_selection.required_fields_title");
            RequiredFieldsMessage = L.Text("fields_selection.required_fields_message");
            UsernameTitle = L.Text("fields_selection.username_title");
            UsernameDescription = L.Text("fields_selection.username_description");
            EmailTitle = L.Text("fields_selection.email_title");
            EmailDescription = L.Text("fields_selection.email_description");
            PreviewTitle = L.Text("fields_selection.preview_title");
            PreviewSubtitle = L.Text("fields_selection.preview_subtitle");
            PreviewAccountTitle = L.Text("fields_selection.preview_account_title");
            PreviewUsername = L.Text("fields_selection.preview_username");
            PreviewEmail = L.Text("fields_selection.preview_email");
            PreviewPassword = L.Text("fields_selection.preview_password");
            ContinueButtonText = L.Text("common.continue_button");
        }


        [RelayCommand]
        private async Task ConfirmSelectionAsync()
        {
            var selectedFields = new Dictionary<string, bool>
                {
                    { "Username", IsUsernameChecked },
                    { "Email", IsEmailChecked },
                };

            var query = new Dictionary<string, object>
                {
                    { "selectedFields", selectedFields },
                };

            if (FolderId != 0)
                query["folderId"] = FolderId;

            await Shell.Current.GoToAsync(nameof(NewAccountPage), query);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("folderId") && int.TryParse(query["folderId"]?.ToString(), out int folderId))
            {
                FolderId = folderId;
            }
        }
    }
}