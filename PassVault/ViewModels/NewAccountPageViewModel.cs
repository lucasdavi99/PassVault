using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Interfaces;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Services;
using PassVault.Views;

namespace PassVault.ViewModels
{
    public partial class NewAccountPageViewModel : ObservableValidator, IQueryAttributable
    {
        private readonly AccountDatabase _database;
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        [Required(ErrorMessage = "Título é obrigatório")]
        private string _title;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        private string _email;

        [ObservableProperty]
        [Required(ErrorMessage = "Senha é obrigatória")]
        private string _password;

        [ObservableProperty]
        private int? folderId;

        [ObservableProperty]
        private Color _selectedColor = Colors.Purple;

        [ObservableProperty]
        private bool _isColorPickerVisible = false;

        [ObservableProperty]
        private string _selectedColorHex = Colors.Purple.ToHex();

        //Campos selecionados.

        [ObservableProperty] private bool isUsernameVisible = true;
        [ObservableProperty] private bool isEmailVisible = true;

        // Localized Properties
        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string pageSubtitle;
        [ObservableProperty] private string accountTitleLabel;
        [ObservableProperty] private string accountTitlePlaceholder;
        [ObservableProperty] private string usernameLabel;
        [ObservableProperty] private string usernamePlaceholder;
        [ObservableProperty] private string emailLabel;
        [ObservableProperty] private string emailPlaceholder;
        [ObservableProperty] private string passwordLabel;
        [ObservableProperty] private string passwordPlaceholder;
        [ObservableProperty] private string colorIdLabel;
        [ObservableProperty] private string colorIdSubtitle;
        [ObservableProperty] private string colorIdTapHere;
        [ObservableProperty] private string saveButtonText;
        [ObservableProperty] private string colorPickerTitle;
        [ObservableProperty] private string colorPickerSubtitle;
        [ObservableProperty] private string hexCodeLabel;
        [ObservableProperty] private string hexCodePlaceholder;
        [ObservableProperty] private string confirmColorButtonText;

        public NewAccountPageViewModel(AccountDatabase database, ILocalizationService localizationService)
        {
            _database = database;
            _localizationService = localizationService;
            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += OnLanguageChanged;

            WeakReferenceMessenger.Default.Register<PasswordGeneratedMessage>(this, (r, m) => { Password = m.Value; });
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = L.Text("new_account.title");
            PageSubtitle = L.Text("new_account.subtitle");
            AccountTitleLabel = L.Text("new_account.account_title_label");
            AccountTitlePlaceholder = L.Text("new_account.account_title_placeholder");
            UsernameLabel = L.Text("new_account.username_label");
            UsernamePlaceholder = L.Text("new_account.username_placeholder");
            EmailLabel = L.Text("new_account.email_label");
            EmailPlaceholder = L.Text("new_account.email_placeholder");
            PasswordLabel = L.Text("new_account.password_label");
            PasswordPlaceholder = L.Text("new_account.password_placeholder");
            ColorIdLabel = L.Text("new_account.color_id_label");
            ColorIdSubtitle = L.Text("new_account.color_id_subtitle");
            ColorIdTapHere = L.Text("new_account.color_id_tap_here");
            SaveButtonText = L.Text("new_account.save_button_text");
            ColorPickerTitle = L.Text("new_account.color_picker_title");
            ColorPickerSubtitle = L.Text("new_account.color_picker_subtitle");
            HexCodeLabel = L.Text("folders.hex_code");
            HexCodePlaceholder = L.Text("new_account.hex_code_placeholder");
            ConfirmColorButtonText = L.Text("folders.confirm_color");
        }

        [RelayCommand]
        private async Task SaveAccountAsync()
        {
            try
            {
                ValidateAllProperties();

                if (HasErrors)
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("new_account.validation_error"), L.Text("common.ok"));
                    return;
                }

                var account = new Account
                {
                    Title = Title,
                    Username = Username,
                    Email = Email,
                    Password = Password,
                    Created = DateTime.Now,
                    Color = SelectedColor.ToHex(),
                    FolderId = FolderId
                };

                await _database.SaveAccountAsync(account);
                await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("messages.account_saved"), L.Text("common.ok"));
                WeakReferenceMessenger.Default.Send(new AccountSavedMessage(true));
                await Shell.Current.Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
            }
        }

        [RelayCommand]
        private async Task GoToGenerator() => await Shell.Current.GoToAsync(nameof(PasswordGenerator));

        [RelayCommand]
        private void ToggleColorPicker()
        {
            IsColorPickerVisible = !IsColorPickerVisible;
        }

        [RelayCommand]
        private void CloseColorPicker()
        {
            IsColorPickerVisible = false;
        }

        partial void OnSelectedColorChanged(Color value)
        {
            // Força a atualização da interface
            SelectedColorHex = value.ToHex();
            OnPropertyChanged(nameof(SelectedColor));
        }

        partial void OnSelectedColorHexChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && (value.Length == 7 || value.Length == 9))
            {
                var newColor = Color.FromArgb(value);
                if (newColor != SelectedColor)
                {
                    SelectedColor = newColor;
                }
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("selectedFields"))
            {
                if (query["selectedFields"] is Dictionary<string, bool> fields)
                {
                    if (fields.TryGetValue("Username", out bool usernameVisible))
                        IsUsernameVisible = usernameVisible;

                    if (fields.TryGetValue("Email", out bool emailVisible))
                        IsEmailVisible = emailVisible;
                }
            }

            if (query.ContainsKey("folderId") && int.TryParse(query["folderId"]?.ToString(), out int folderId))
            {
                FolderId = folderId;
            }
        }
    }
}