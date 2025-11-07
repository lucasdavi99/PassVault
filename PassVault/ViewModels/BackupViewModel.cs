using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using PassVault.Data;
using PassVault.exceptions;
using PassVault.Interfaces;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Services;
using CommunityToolkit.Maui.Alerts;

namespace PassVault.ViewModels
{
    public partial class BackupViewModel : ObservableObject
    {
        private readonly ExportService _exportService;
        private readonly ImportService _importService;
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;
        private readonly ILocalizationService _localizationService;

        [ObservableProperty] private string exportFilePath;
        [ObservableProperty] private string exportPassword;
        [ObservableProperty] private string importFilePath;
        [ObservableProperty] private string importPassword;
        [ObservableProperty] private bool isPasswordVisible;

        // Propriedades localizadas
        [ObservableProperty] private string backupTitle;
        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string pageSubtitle;
        [ObservableProperty] private string exportTitle;
        [ObservableProperty] private string exportSubtitle;
        [ObservableProperty] private string exportPasswordLabel;
        [ObservableProperty] private string exportButtonText;
        [ObservableProperty] private string importTitle;
        [ObservableProperty] private string importSubtitle;
        [ObservableProperty] private string importButtonText;
        [ObservableProperty] private string securityInfoTitle;
        [ObservableProperty] private string copyPasswordButton;
        [ObservableProperty] private string securityInfo1;
        [ObservableProperty] private string securityInfo2;
        [ObservableProperty] private string securityInfo3;
        [ObservableProperty] private string securityInfo4;


        public BackupViewModel(ExportService exportService, ImportService importService, AccountDatabase accountDatabase, FolderDatabase folderDatabase, ILocalizationService localizationService)
        {
            _exportService = exportService;
            _importService = importService;
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
            _localizationService = localizationService;
            IsPasswordVisible = false;

            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(UpdateLocalizedTexts);
        }

        private void UpdateLocalizedTexts()
        {
            BackupTitle = L.Text("backup.title");
            PageTitle = L.Text("backup.page_title");
            PageSubtitle = L.Text("backup.page_subtitle");
            ExportTitle = L.Text("backup.export_title");
            ExportSubtitle = L.Text("backup.export_subtitle");
            ExportPasswordLabel = L.Text("backup.export_password_label");
            ExportButtonText = L.Text("backup.export_button_text");
            ImportTitle = L.Text("backup.import_title");
            ImportSubtitle = L.Text("backup.import_subtitle");
            ImportButtonText = L.Text("backup.import_button_text");
            SecurityInfoTitle = L.Text("backup.security_info_title");
            CopyPasswordButton = L.Text("backup.copy_password_button");
            SecurityInfo1 = L.Text("backup.security_info1");
            SecurityInfo2 = L.Text("backup.security_info2");
            SecurityInfo3 = L.Text("backup.security_info3");
            SecurityInfo4 = L.Text("backup.security_info4");
        }

        [RelayCommand]
        public async Task ExportBackupAsync()
        {
            var accounts = await _accountDatabase.GetAccountsAsync();
            var folders = await _folderDatabase.GetFoldersAsync();

            if (accounts.Count > 0 || folders.Count > 0)
            {
                var (filePath, password) = await _exportService.ExportBackupAsync(accounts, folders);

                ExportFilePath = filePath;
                ExportPassword = password;
                IsPasswordVisible = false;

                bool sharingCompleted = await Shell.Current.DisplayAlert(L.Text("export.confirm_title"), L.Text("export.confirm_message"), L.Text("common.yes"), L.Text("common.no"));

                if (sharingCompleted)
                {
                    IsPasswordVisible = true;
                    await Shell.Current.DisplayAlert(L.Text("export.success_title"), L.Text("export.success_message"), L.Text("common.ok"));
                }
                else
                {
                    if (File.Exists(ExportFilePath))
                    {
                        File.Delete(ExportFilePath);
                    }
                    ExportPassword = string.Empty;
                    await Shell.Current.DisplayAlert(L.Text("export.cancel_title"), L.Text("export.cancel_message"), L.Text("common.ok"));
                }
            }
            else
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("export.empty_message"), L.Text("common.ok"));
            }
        }

        [RelayCommand]
        public async Task ImportBackupAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = L.Text("import.picker_title"),
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "application/octet-stream" } },
                        { DevicePlatform.iOS, new[] { "public.data" } },
                        { DevicePlatform.WinUI, new[] { ".dat" } }
                    })
                });

                if (result == null) return;

                string filePath = result.FullPath;

                if (string.IsNullOrEmpty(filePath))
                {
                    using var stream = await result.OpenReadAsync();
                    filePath = Path.Combine(FileSystem.AppDataDirectory, result.FileName);
                    using var fileStream = File.Create(filePath);
                    await stream.CopyToAsync(fileStream);
                }

                ImportFilePath = filePath;

                string senha = await Shell.Current.DisplayPromptAsync(L.Text("import.password_prompt_title"), L.Text("import.password_prompt_message"));
                ImportPassword = senha;

                if (string.IsNullOrWhiteSpace(senha))
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("import.password_required"), L.Text("common.ok"));
                    return;
                }

                BackupData backupData = await _importService.ImportBackupAsync(filePath, senha);

                Dictionary<int, int> folderIdMapping = new Dictionary<int, int>();

                foreach (var folder in backupData.Folders)
                {
                    int oldId = folder.Id;

                    folder.Id = 0;
                    await _folderDatabase.SaveFolderAsync(folder);

                    int newId = folder.Id;
                    folderIdMapping.Add(oldId, newId);
                }

                foreach (var account in backupData.Accounts)
                {
                    if (account.FolderId.HasValue && folderIdMapping.ContainsKey(account.FolderId.Value))
                    {
                        account.FolderId = folderIdMapping[account.FolderId.Value];
                    }
                    else
                    {
                        account.FolderId = null;
                    }

                    account.Id = 0;
                    await _accountDatabase.SaveAccountAsync(account);
                }

                WeakReferenceMessenger.Default.Send(new AccountSavedMessage(true));
                WeakReferenceMessenger.Default.Send(new FolderSavedMessage(true));

                await Shell.Current.DisplayAlert(L.Text("import.success_title"), L.Text("import.success_message"), L.Text("common.ok"));
                await Shell.Current.Navigation.PopAsync();
            }
            catch (InvalidImportPasswordException)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("import.wrong_password"), L.Text("common.ok"));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
            }
        }

        [RelayCommand]
        private async Task CopyExportPasswordAsync()
        {
            if (string.IsNullOrEmpty(ExportPassword))
                return;

            if (await TryCopyToClipboardAsync(ExportPassword))
            {
                await Snackbar.Make(CopyPasswordButton, duration: TimeSpan.FromSeconds(2)).Show();
            }
        }

        ~BackupViewModel()
        {
            if (_localizationService != null)
                _localizationService.LanguageChanged -= OnLanguageChanged;
        }

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
    }
}
