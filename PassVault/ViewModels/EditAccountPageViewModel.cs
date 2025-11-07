using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using PassVault.Data;
using PassVault.Interfaces;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Services;
using PassVault.Services.Security;
using PassVault.Views;

namespace PassVault.ViewModels
{
    public partial class EditAccountPageViewModel : ObservableValidator, IQueryAttributable
    {
        private readonly AccountDatabase _database;
        private readonly FolderDatabase _folderDatabase;
        private readonly ILocalizationService _localizationService;
        private readonly IAuthenticationService _authenticationService;
        private Account _currentAccount;

        [ObservableProperty] private int _accountId;
        [ObservableProperty][Required(ErrorMessage = "Título é obrigatório")] private string _title;
        [ObservableProperty] private string _username;
        [ObservableProperty][EmailAddress(ErrorMessage = "E-mail inválido")] private string _email;
        [ObservableProperty][Required(ErrorMessage = "Senha é obrigatória")] private string _password;
        [ObservableProperty] private Color _selectedColor = Colors.Purple;
        [ObservableProperty] private string _selectedColorHex = Colors.Purple.ToHex();
        [ObservableProperty] private bool _isColorPickerVisible = false;
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private bool _isPasswordVisible = false;
        [ObservableProperty] private List<Folder> _folders = new();
        [ObservableProperty] private string _selectedFolderName;
        [ObservableProperty] private bool isUsernameVisible = true;
        [ObservableProperty] private bool isEmailVisible = true;
        [ObservableProperty] private Folder _selectedParentFolder;
        [ObservableProperty] private List<Folder> _subFolders = new();
        [ObservableProperty] private string _selectedSubFolderName;
        [ObservableProperty] private bool _hasSubFolderButtonVisible = false;

        // Localized Properties
        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string headerTitle;
        [ObservableProperty] private string headerSubtitle;
        [ObservableProperty] private string formSectionTitle;
        [ObservableProperty] private string formSectionSubtitle;
        [ObservableProperty] private string accountNameLabel;
        [ObservableProperty] private string accountNamePlaceholder;
        [ObservableProperty] private string usernameLabel;
        [ObservableProperty] private string usernamePlaceholder;
        [ObservableProperty] private string emailLabel;
        [ObservableProperty] private string emailPlaceholder;
        [ObservableProperty] private string passwordLabel;
        [ObservableProperty] private string passwordPlaceholder;
        [ObservableProperty] private string colorLabel;
        [ObservableProperty] private string organizationLabel;
        [ObservableProperty] private string mainFolderLabel;
        [ObservableProperty] private string subFolderLabel;
        [ObservableProperty] private string editButtonText;
        [ObservableProperty] private string cancelButtonText;
        [ObservableProperty] private string saveButtonText;
        [ObservableProperty] private string customColorTitle;
        [ObservableProperty] private string customColorSubtitle;
        [ObservableProperty] private string selectedColorLabel;
        [ObservableProperty] private string applyColorButtonText;
        [ObservableProperty] private string fieldOptionsTitle;
        [ObservableProperty] private string fieldOptionsSubtitle;
        [ObservableProperty] private string usernameToggleLabel;
        [ObservableProperty] private string emailToggleLabel;


        public EditAccountPageViewModel(AccountDatabase database, FolderDatabase folderDatabase, ILocalizationService localizationService, IAuthenticationService authenticationService)
        {
            _database = database;
            _folderDatabase = folderDatabase;
            _localizationService = localizationService;
            _authenticationService = authenticationService;
            IsEditing = false;

            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += OnLanguageChanged;

            WeakReferenceMessenger.Default.Register<PasswordGeneratedMessage>(this, async (r, m) =>
            {
                await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("messages.password_applied_success"), L.Text("common.ok"));
                Password = m.Value;
            });
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(UpdateLocalizedTexts);
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = L.Text("edit_account_page.title");
            HeaderTitle = L.Text("edit_account_page.header_title");
            HeaderSubtitle = IsEditing ? L.Text("edit_account_page.header_subtitle_edit") : L.Text("edit_account_page.header_subtitle_view");
            FormSectionTitle = L.Text("edit_account_page.form_title");
            FormSectionSubtitle = L.Text("edit_account_page.form_subtitle");
            AccountNameLabel = L.Text("edit_account_page.account_name_label");
            AccountNamePlaceholder = L.Text("edit_account_page.account_name_placeholder");
            UsernameLabel = L.Text("edit_account_page.username_label");
            UsernamePlaceholder = L.Text("edit_account_page.username_placeholder");
            EmailLabel = L.Text("edit_account_page.email_label");
            EmailPlaceholder = L.Text("edit_account_page.email_placeholder");
            PasswordLabel = L.Text("edit_account_page.password_label");
            PasswordPlaceholder = L.Text("edit_account_page.password_placeholder");
            ColorLabel = L.Text("edit_account_page.color_label");
            OrganizationLabel = L.Text("edit_account_page.organization_label");
            MainFolderLabel = L.Text("edit_account_page.main_folder_label");
            SubFolderLabel = L.Text("edit_account_page.subfolder_label");
            EditButtonText = L.Text("edit_account_page.edit_button");
            CancelButtonText = L.Text("edit_account_page.cancel_button");
            SaveButtonText = L.Text("edit_account_page.save_button");
            CustomColorTitle = L.Text("edit_account_page.custom_color_title");
            CustomColorSubtitle = L.Text("edit_account_page.custom_color_subtitle");
            SelectedColorLabel = L.Text("edit_account_page.selected_color_label");
            ApplyColorButtonText = L.Text("edit_account_page.apply_color_button");
            FieldOptionsTitle = L.Text("edit_account_page.field_options_title");
            FieldOptionsSubtitle = L.Text("edit_account_page.field_options_subtitle");
            UsernameToggleLabel = L.Text("edit_account_page.username_toggle_label");
            EmailToggleLabel = L.Text("edit_account_page.email_toggle_label");

            SelectedFolderName = L.Text("accounts.select_folder");
            SelectedSubFolderName = L.Text("accounts.select_subfolder");
        }


        [RelayCommand]
        private async Task SelectFolderAsync()
        {
            Folders = await _folderDatabase.GetRootFoldersAsync();

            if (Folders == null || Folders.Count == 0)
            {
                await Shell.Current.DisplayAlert(L.Text("common.warning"), L.Text("messages.no_folders_found"), L.Text("common.ok"));
                return;
            }

            var sortedFolders = Folders
               .OrderBy(f => f.Title, StringComparer.OrdinalIgnoreCase)
               .ToList();
            Folders = sortedFolders;

            var folderNames = Folders.Select(f => f.Title).ToList();
            folderNames.Insert(0, L.Text("accounts.no_folder"));

            string chosenOption = await Shell.Current.DisplayActionSheet(L.Text("accounts.select_folder"), L.Text("common.cancel"), null, folderNames.ToArray());

            if (chosenOption != null && chosenOption != L.Text("common.cancel"))
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    L.Text("messages.confirm_move_account_title"),
                    string.Format(L.Text("messages.confirm_move_account"), chosenOption),
                    L.Text("common.yes"),
                    L.Text("common.cancel")
                );

                if (confirm)
                {
                    if (chosenOption == L.Text("accounts.no_folder"))
                    {
                        SelectedFolderName = L.Text("accounts.no_folder");
                        SelectedParentFolder = null;
                        SelectedSubFolderName = L.Text("accounts.select_subfolder");
                        HasSubFolderButtonVisible = false;
                        SubFolders.Clear();
                        _currentAccount.FolderId = null;
                    }
                    else
                    {
                        SelectedFolderName = chosenOption;
                        SelectedParentFolder = Folders.First(f => f.Title == chosenOption);
                        _currentAccount.FolderId = SelectedParentFolder.Id;
                        SelectedSubFolderName = L.Text("accounts.select_subfolder");
                        await LoadSubFoldersAsync();
                    }
                }
            }
        }

        [RelayCommand]
        private async Task SelectSubFolderAsync()
        {
            if (SelectedParentFolder == null || SubFolders == null || SubFolders.Count == 0)
            {
                await Shell.Current.DisplayAlert(L.Text("common.warning"), L.Text("messages.no_subfolders_found"), L.Text("common.ok"));
                return;
            }

            var sortedSubFolders = SubFolders
                .OrderBy(f => f.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var subFolderNames = sortedSubFolders.Select(f => f.Title).ToList();
            subFolderNames.Insert(0, L.Text("accounts.keep_in_main_folder"));

            string chosenOption = await Shell.Current.DisplayActionSheet(L.Text("accounts.select_subfolder"), L.Text("common.cancel"), null, subFolderNames.ToArray());

            if (chosenOption != null && chosenOption != L.Text("common.cancel"))
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    L.Text("messages.confirm_move_account_title"),
                    string.Format(L.Text("messages.confirm_move_account_subfolder"), chosenOption),
                    L.Text("common.yes"),
                    L.Text("common.cancel")
                );

                if (confirm)
                {
                    if (chosenOption == L.Text("accounts.keep_in_main_folder"))
                    {
                        SelectedSubFolderName = L.Text("accounts.main_folder");
                        _currentAccount.FolderId = SelectedParentFolder.Id;
                    }
                    else
                    {
                        SelectedSubFolderName = chosenOption;
                        var selectedSubFolder = sortedSubFolders.First(f => f.Title == chosenOption);
                        _currentAccount.FolderId = selectedSubFolder.Id;
                    }
                }
            }
        }

        private async Task LoadSubFoldersAsync()
        {
            if (SelectedParentFolder == null)
            {
                HasSubFolderButtonVisible = false;
                return;
            }

            try
            {
                SubFolders = await _folderDatabase.GetSubFoldersAsync(SelectedParentFolder.Id);
                HasSubFolderButtonVisible = SubFolders != null && SubFolders.Count > 0;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), string.Format(L.Text("messages.error_loading_subfolders"), ex.Message), L.Text("common.ok"));
                HasSubFolderButtonVisible = false;
            }
        }

        [RelayCommand]
        private async Task SaveEditAccountAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.fill_required_fields"), L.Text("common.ok"));
                    return;
                }

                var sanitizedTitle = Title?.Trim();

                // Verificar se já existe uma conta com o mesmo nome na mesma pasta (excluindo a conta atual)
                bool accountExists = await _database.AccountNameExistsAsync(sanitizedTitle, _currentAccount.FolderId, _currentAccount.Id);
                
                if (accountExists)
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.duplicate_account_name"), L.Text("common.ok"));
                    return;
                }

                var sanitizedUsername = IsUsernameVisible ? Username?.Trim() : null;
                var sanitizedEmail = IsEmailVisible ? Email?.Trim() : null;

                _currentAccount.Title = sanitizedTitle;
                _currentAccount.Username = string.IsNullOrWhiteSpace(sanitizedUsername) ? null : sanitizedUsername;
                _currentAccount.Email = string.IsNullOrWhiteSpace(sanitizedEmail) ? null : sanitizedEmail;
                _currentAccount.Password = Password;
                _currentAccount.Color = SelectedColor.ToHex();

                await _database.SaveAccountAsync(_currentAccount);
                await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("messages.account_updated_success"), L.Text("common.ok"));
                WeakReferenceMessenger.Default.Send(new AccountSavedMessage(true));
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
            }
        }

        [RelayCommand]
        private async Task GoToGenerator() => await Shell.Current.GoToAsync(nameof(PasswordGenerator));

        [RelayCommand]
        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        [RelayCommand]
        private async Task CopyPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(Password))
                return;

            if (await TryCopyToClipboardAsync(Password))
            {
                await Snackbar.Make(L.Text("password_copied"), duration: TimeSpan.FromSeconds(2)).Show();
            }
        }

        [RelayCommand]
        private void ToggleColorPicker() => IsColorPickerVisible = !IsColorPickerVisible;

        [RelayCommand]
        private void CloseColorPicker() => IsColorPickerVisible = false;

        private static async Task<bool> TryCopyToClipboardAsync(string text)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() => Clipboard.SetTextAsync(text));
                return true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
                return false;
            }
        }

        [RelayCommand]
        private async Task<bool> ToggleEditMode()
        {
            try
            {
                var request = new AppAuthenticationRequest
                {
                    Title = L.Text("messages.auth_needed_title"),
                    Message = L.Text("messages.auth_needed_message"),
                    AllowAlternativeAuthentication = true,
                    CancelTitle = L.Text("common.cancel"),
                    FallbackTitle = L.Text("security.master_password")
                };

                var authResult = await _authenticationService.AuthenticateAsync(request);

                if (authResult.Status == AppAuthenticationStatus.NotAvailable)
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.no_password_configured"), L.Text("common.ok"));
                    return false;
                }

                if (authResult.IsSuccessful)
                {
                    IsEditing = !IsEditing;
                    HeaderSubtitle = IsEditing ? L.Text("edit_account_page.header_subtitle_edit") : L.Text("edit_account_page.header_subtitle_view");
                    return true;
                }

                var errorMessage = !string.IsNullOrWhiteSpace(authResult.ErrorMessage)
                    ? string.Format(L.Text("messages.auth_error"), authResult.ErrorMessage)
                    : L.Text("messages.auth_failed");

                await Shell.Current.DisplayAlert(L.Text("common.error"), errorMessage, L.Text("common.ok"));
                return false;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), string.Format(L.Text("messages.auth_error"), ex.Message), L.Text("common.ok"));
                return false;
            }
        }

        partial void OnSelectedColorChanged(Color value)
        {
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

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("accountId") && int.TryParse(query["accountId"]?.ToString(), out int accountId))
            {
                AccountId = accountId;
                _currentAccount = await _database.GetAccountAsync(accountId);

                if (_currentAccount != null)
                {
                    Title = _currentAccount.Title;
                    Username = _currentAccount.Username ?? string.Empty;
                    Email = _currentAccount.Email ?? string.Empty;
                    Password = _currentAccount.Password;
                    SelectedColor = Color.FromArgb(_currentAccount.Color);
                    IsUsernameVisible = !string.IsNullOrWhiteSpace(_currentAccount.Username);
                    IsEmailVisible = !string.IsNullOrWhiteSpace(_currentAccount.Email);
                    await ConfigureFolderDisplayAsync();
                }

                if (query.ContainsKey("selectedFields") && query["selectedFields"] is Dictionary<string, bool> selectedFields)
                {
                    IsUsernameVisible = selectedFields.GetValueOrDefault("Username", true);
                    IsEmailVisible = selectedFields.GetValueOrDefault("Email", true);
                }
            }
        }

        private async Task ConfigureFolderDisplayAsync()
        {
            if (_currentAccount.FolderId == null)
            {
                SelectedFolderName = L.Text("accounts.no_folder");
                SelectedParentFolder = null;
                SelectedSubFolderName = L.Text("accounts.select_subfolder");
                HasSubFolderButtonVisible = false;
                return;
            }

            try
            {
                var currentFolder = await _folderDatabase.GetFolderAsync(_currentAccount.FolderId.Value);

                if (currentFolder == null)
                {
                    SelectedFolderName = L.Text("accounts.folder_not_found");
                    return;
                }

                if (currentFolder.ParentFolderId == null)
                {
                    SelectedFolderName = currentFolder.Title;
                    SelectedParentFolder = currentFolder;
                    SelectedSubFolderName = L.Text("accounts.main_folder");
                    await LoadSubFoldersAsync();
                }
                else
                {
                    var parentFolder = await _folderDatabase.GetFolderAsync(currentFolder.ParentFolderId.Value);
                    SelectedFolderName = parentFolder?.Title ?? L.Text("accounts.folder_not_found");
                    SelectedParentFolder = parentFolder;
                    SelectedSubFolderName = currentFolder.Title;
                    await LoadSubFoldersAsync();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), string.Format(L.Text("messages.error_loading_folders"), ex.Message), L.Text("common.ok"));
            }
        }

        ~EditAccountPageViewModel()
        {
            if (_localizationService != null)
                _localizationService.LanguageChanged -= OnLanguageChanged;
        }
    }
}
