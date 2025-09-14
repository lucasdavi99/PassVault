using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Interfaces;
using PassVault.Services;
using System.Collections.ObjectModel;

namespace PassVault.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;
        private readonly CacheService _cacheService;
        // ✅ ADICIONADO: Campo do serviço de localização
        private readonly ILocalizationService _localizationService;

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

        // ✅ ADICIONADO: Propriedades localizadas
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

        // ✅ MODIFICADO: Construtor com novo parâmetro
        public SettingsPageViewModel(
            AccountDatabase accountDatabase,
            FolderDatabase folderDatabase,
            CacheService cacheService,
            ILocalizationService localizationService)
        {
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
            _cacheService = cacheService;
            // ✅ ADICIONADO: Inicialização do serviço
            _localizationService = localizationService;

            LoadCurrentLanguage();
            LoadAppInfo();

            // ✅ ADICIONADO: Configuração da localização
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

        // ✅ MODIFICADO: Método ApplyLanguage atualizado
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

        // ✅ ADICIONADO: Métodos de localização
        private void UpdateLocalizedTexts()
        {
            SettingsTitle = L.Text("settings.title");
            SettingsSubtitle = L.Text("settings.subtitle");
            LanguageTitle = L.Text("settings.language.title");
            LanguageSubtitle = L.Text("settings.language.subtitle");
            CurrentLanguageText = L.Text("settings.language.current");
            ApplyLanguageText = L.Text("settings.language.apply");
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            UpdateLocalizedTexts();
        }

        // ✅ ADICIONADO: Método para reset do app (se necessário)
        [RelayCommand]
        private async Task ResetApp()
        {
            try
            {
                var title = "Redefinir Aplicativo";
                var message = "Tem certeza de que deseja limpar todos os dados? Esta ação não pode ser desfeita.";
                var yesText = L.Text("common.yes");
                var noText = L.Text("common.no");

                var result = await Shell.Current.DisplayAlert(title, message, yesText, noText);

                if (result)
                {
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
                    Preferences.Set("AppLanguage", currentLanguage); // Manter idioma se quiser

                    // 5. Forçar coleta de lixo
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    var successTitle = L.Text("common.success");
                    var successMessage = "Dados limpos com sucesso!";
                    var okText = L.Text("common.ok");

                    await Shell.Current.DisplayAlert(successTitle, successMessage, okText);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao resetar app: {ex.Message}");

                var errorTitle = L.Text("common.error");
                var errorMessage = "Erro ao limpar os dados. Tente novamente.";
                var okText = L.Text("common.ok");

                await Shell.Current.DisplayAlert(errorTitle, errorMessage, okText);
            }
        }

        // ✅ ADICIONADO: Dispose para limpar eventos
        public void Dispose()
        {
            if (_localizationService != null)
                _localizationService.LanguageChanged -= OnLanguageChanged;
        }
    }
}