using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Interfaces;
using PassVault.Services;
using PassVault.Services.Security;
using System.Collections.ObjectModel;

namespace PassVault.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;
        private readonly CacheService _cacheService;
        private readonly ILocalizationService _localizationService;
        private readonly IAuthenticationService _authenticationService;

        [ObservableProperty]
        private ObservableCollection<string> availableLanguages = new()
        {
            "Português (Brasil)",
            "English (US)"
        };

        [ObservableProperty]
        private string selectedLanguage = "Português (Brasil)";

        // Propriedades dinâmicas do app
        [ObservableProperty]
        private string appVersion;

        [ObservableProperty]
        private string appName;

        [ObservableProperty]
        private string buildNumber;

        // Propriedades localizadas - Configurações
        [ObservableProperty]
        private string settingsTitle;

        [ObservableProperty]
        private string settingsSubtitle;

        // Propriedades localizadas - Idioma
        [ObservableProperty]
        private string languageTitle;

        [ObservableProperty]
        private string languageSubtitle;

        [ObservableProperty]
        private string currentLanguageText;

        [ObservableProperty]
        private string applyLanguageText;

        // Propriedades localizadas - Reset
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

        // Propriedades localizadas - Sobre
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
            IAuthenticationService authenticationService)
        {
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
            _cacheService = cacheService;
            _localizationService = localizationService;
            _authenticationService = authenticationService;

            LoadCurrentLanguage();
            LoadAppInfo();

            // Configuração da localização
            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += OnLanguageChanged;
        }

        private void LoadCurrentLanguage()
        {
            // Carregar idioma salvo nas preferências
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
                // Fallback caso não consiga acessar as informações
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

                // Aplicar o idioma através do serviço
                await _localizationService.SetLanguageAsync(languageCode);

                // Mostrar mensagem de confirmação
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
            // Configurações gerais
            SettingsTitle = L.Text("settings.title");
            SettingsSubtitle = L.Text("settings.subtitle");
            
            // Seção de idioma
            LanguageTitle = L.Text("settings.language.title");
            LanguageSubtitle = L.Text("settings.language.subtitle");
            CurrentLanguageText = L.Text("settings.language.current");
            ApplyLanguageText = L.Text("settings.language.apply");
            
            // Seção de reset
            ResetTitle = L.Text("settings.reset.title");
            ResetSubtitle = L.Text("settings.reset.subtitle");
            WarningTitle = L.Text("settings.reset.warning_title");
            WarningMessage = L.Text("settings.reset.warning_message");
            ResetAppText = L.Text("settings.reset.button");
            
            // Seção sobre
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

                var result = await Shell.Current.DisplayAlert(title, message, yesText, noText);

                if (result)
                {
                    if (!await EnsureWindowsAuthorizationAsync())
                        return;

                    // 1. Limpar cache
                    _cacheService.ClearAll();

                    // 2. Deletar todas as contas
                    var accounts = await _accountDatabase.GetAccountsAsync();
                    foreach (var account in accounts)
                    {
                        await _accountDatabase.DeleteAccountAsync(account);
                    }

                    // 3. Deletar todas as pastas
                    var folders = await _folderDatabase.GetFoldersAsync();
                    foreach (var folder in folders)
                    {
                        await _folderDatabase.DeleteFolderAsync(folder);
                    }

                    // 4. Limpar todas as preferências (exceto o idioma se quiser manter)
                    var currentLanguage = Preferences.Get("AppLanguage", "pt-BR");

                    Preferences.Clear();

                    // Restaurar configurações iniciais
                    Preferences.Set("IsNewUser", true);
                    Preferences.Set("AppLanguage", currentLanguage); // Manter idioma

                    // 5. Forçar coleta de lixo
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

        private async Task<bool> EnsureWindowsAuthorizationAsync()
        {
            if (!OperatingSystem.IsWindows())
                return true;

            var request = new AppAuthenticationRequest
            {
                Title = L.Text("messages.auth_needed_title"),
                Message = L.Text("messages.auth_needed_message"),
                AllowAlternativeAuthentication = true
            };

            var result = await _authenticationService.AuthenticateAsync(request);

            if (result.Status == AppAuthenticationStatus.NotAvailable)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.no_password_configured"), L.Text("common.ok"));
                return false;
            }

            if (!result.IsSuccessful)
            {
                var message = !string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? string.Format(L.Text("messages.auth_error"), result.ErrorMessage)
                    : L.Text("messages.auth_failed");

                await Shell.Current.DisplayAlert(L.Text("common.error"), message, L.Text("common.ok"));
            }

            return result.IsSuccessful;
        }

        // Dispose para limpar eventos
        ~SettingsPageViewModel()
        {
            if (_localizationService != null)
                _localizationService.LanguageChanged -= OnLanguageChanged;
        }
    }
}
