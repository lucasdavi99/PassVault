using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Interfaces;
using PassVault.Services;
using PassVault.Views; // Certifique-se que a view UpgradePage está neste namespace
using System.Collections.ObjectModel;

namespace PassVault.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;
        private readonly CacheService _cacheService;
        private readonly ILocalizationService _localizationService;
        private readonly IVipService _vipService; // Serviço VIP injetado

        // --- Novas Propriedades para a Seção VIP ---
        [ObservableProperty]
        private bool isNotVip;

        [ObservableProperty]
        private string vipSectionTitle;

        [ObservableProperty]
        private string vipSectionSubtitle;

        [ObservableProperty]
        private string goToUpgradeButtonText;

        [ObservableProperty]
        private string vipBenefitsTitle;

        [ObservableProperty]
        private string vipBenefitUnlimitedAccounts;

        [ObservableProperty]
        private string vipBenefitUnlimitedFolders;

        [ObservableProperty]
        private string vipBenefitSubfolders;
        // --- Fim das Propriedades VIP ---

        [ObservableProperty]
        private ObservableCollection<string> availableLanguages = new()
        {
            "Português (Brasil)",
            "English (US)"
        };

        [ObservableProperty]
        private string selectedLanguage = "Português (Brasil)";

        [ObservableProperty]
        private string appVersion;

        [ObservableProperty]
        private string appName;

        [ObservableProperty]
        private string buildNumber;

        [ObservableProperty]
        private string settingsTitle;

        [ObservableProperty]
        private string settingsSubtitle;

        [ObservableProperty]
        private string languageTitle;

        [ObservableProperty]
        private string languageSubtitle;

        [ObservableProperty]
        private string currentLanguageText;

        [ObservableProperty]
        private string applyLanguageText;

        [ObservableProperty]
        private string resetTitle;

        [ObservableProperty]
        private string resetSubtitle;

        [ObservableProperty]
        private string warningTitle;

        [ObservableProperty]
        private string warningMessage;

        [ObservableProperty]
        private string resetAppText;

        [ObservableProperty]
        private string aboutTitle;

        [ObservableProperty]
        private string aboutSubtitle;

        [ObservableProperty]
        private string versionText;

        [ObservableProperty]
        private string developerText;

        public SettingsPageViewModel(
            AccountDatabase accountDatabase,
            FolderDatabase folderDatabase,
            CacheService cacheService,
            ILocalizationService localizationService,
            IVipService vipService) // Injeção do IVipService
        {
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
            _cacheService = cacheService;
            _localizationService = localizationService;
            _vipService = vipService; // Armazenar a instância

            LoadCurrentLanguage();
            LoadAppInfo();
            LoadVipStatus(); // Carregar o status do VIP

            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += OnLanguageChanged;
        }

        //Comando para ativar modo premiun (apenas Dev)
        [RelayCommand]
        private async Task ToggleVipStatus()
        {
            bool isCurrentlyVip = _vipService.IsUserVip();
            _vipService.SetUserVipStatus(!isCurrentlyVip);

            string status = _vipService.IsUserVip() ? "ATIVADO" : "DESATIVADO";
            await Shell.Current.DisplayAlert("Status de Teste", $"O modo VIP foi {status}.", "OK");
            await Shell.Current.Navigation.PopAsync(); // Volta para a tela anterior para ver o resultado
        }

        private void LoadVipStatus()
        {
            IsNotVip = !_vipService.IsUserVip();
        }

        private void LoadCurrentLanguage()
        {
            var savedLanguage = Preferences.Get("AppLanguage", "pt-BR");
            SelectedLanguage = savedLanguage switch
            {
                "en-US" => "English (US)",
                _ => "Português (Brasil)"
            };
        }

        private void LoadAppInfo()
        {
            try
            {
                AppVersion = AppInfo.VersionString;
                AppName = AppInfo.Name;
                BuildNumber = AppInfo.BuildString;
            }
            catch (Exception ex)
            {
                AppVersion = "1.2.0";
                AppName = "PassVault";
                BuildNumber = "1";
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar AppInfo: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ApplyLanguage()
        {
            try
            {
                var languageCode = SelectedLanguage switch
                {
                    "English (US)" => "en-US",
                    _ => "pt-BR"
                };

                await _localizationService.SetLanguageAsync(languageCode);

                var title = L.Text("settings.language.changed");
                var message = L.Text("settings.language.restart_message");
                var okText = L.Text("common.ok");
                await Shell.Current.DisplayAlert(title, message, okText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao aplicar idioma: {ex.Message}");
                var errorTitle = L.Text("common.error");
                var errorMessage = "Erro ao aplicar o idioma. Tente novamente.";
                var okText = L.Text("common.ok");
                await Shell.Current.DisplayAlert(errorTitle, errorMessage, okText);
            }
        }

        private void UpdateLocalizedTexts()
        {
            SettingsTitle = L.Text("settings.title");
            SettingsSubtitle = L.Text("settings.subtitle");

            // Textos da Seção VIP
            VipSectionTitle = L.Text("settings.vip.title");
            VipSectionSubtitle = L.Text("settings.vip.subtitle");
            GoToUpgradeButtonText = L.Text("settings.vip.button");
            VipBenefitsTitle = L.Text("upgrade_page.benefits_title");
            VipBenefitUnlimitedAccounts = L.Text("upgrade_page.benefit_unlimited_accounts");
            VipBenefitUnlimitedFolders = L.Text("upgrade_page.benefit_unlimited_folders");
            VipBenefitSubfolders = L.Text("upgrade_page.benefit_subfolders");

            LanguageTitle = L.Text("settings.language.title");
            LanguageSubtitle = L.Text("settings.language.subtitle");
            CurrentLanguageText = L.Text("settings.language.current");
            ApplyLanguageText = L.Text("settings.language.apply");

            ResetTitle = L.Text("settings.reset.title");
            ResetSubtitle = L.Text("settings.reset.subtitle");
            WarningTitle = L.Text("settings.reset.warning_title");
            WarningMessage = L.Text("settings.reset.warning_message");
            ResetAppText = L.Text("settings.reset.button");

            AboutTitle = L.Text("settings.about.title");
            AboutSubtitle = L.Text("settings.about.subtitle");
            VersionText = L.Text("settings.about.version");
            DeveloperText = L.Text("settings.about.developer");
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateLocalizedTexts();
            });
        }

        [RelayCommand]
        private async Task ResetApp()
        {
            try
            {
                var title = L.Text("settings.reset.confirm_title");
                var message = L.Text("settings.reset.confirm_message");
                var yesText = L.Text("common.yes");
                var noText = L.Text("common.no");

                if (await Shell.Current.DisplayAlert(title, message, yesText, noText))
                {
                    _cacheService.ClearAll();

                    var accounts = await _accountDatabase.GetAccountsAsync();
                    foreach (var account in accounts)
                    {
                        await _accountDatabase.DeleteAccountAsync(account);
                    }

                    var folders = await _folderDatabase.GetFoldersAsync();
                    foreach (var folder in folders)
                    {
                        await _folderDatabase.DeleteFolderAsync(folder);
                    }

                    var currentLanguage = Preferences.Get("AppLanguage", "pt-BR");
                    Preferences.Clear();

                    Preferences.Set("IsNewUser", true);
                    Preferences.Set("AppLanguage", currentLanguage);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    var successTitle = L.Text("common.success");
                    var successMessage = L.Text("settings.reset.success_message");
                    var okText = L.Text("common.ok");
                    await Shell.Current.DisplayAlert(successTitle, successMessage, okText);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao resetar app: {ex.Message}");
                var errorTitle = L.Text("common.error");
                var errorMessage = L.Text("settings.reset.error_message");
                var okText = L.Text("common.ok");
                await Shell.Current.DisplayAlert(errorTitle, errorMessage, okText);
            }
        }

        // --- Novo Comando para Navegação ---
        [RelayCommand]
        private async Task GoToUpgradePage()
        {
            if (IsNotVip)
            {
                await Shell.Current.GoToAsync(nameof(UpgradePage));
            }
        }

        ~SettingsPageViewModel()
        {
            if (_localizationService != null)
                _localizationService.LanguageChanged -= OnLanguageChanged;
        }
    }
}
