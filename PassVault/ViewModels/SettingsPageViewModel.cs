using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Services;
using System.Collections.ObjectModel;

namespace PassVault.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;
        private readonly CacheService _cacheService;

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

        public SettingsPageViewModel(
            AccountDatabase accountDatabase,
            FolderDatabase folderDatabase,
            CacheService cacheService)
        {
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
            _cacheService = cacheService;

            LoadCurrentLanguage();
            LoadAppInfo();
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
            }
            catch (Exception ex)
            {
                // Fallback caso não consiga acessar as informações
                AppVersion = "1.2.0";

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

                // Salvar nas preferências
                Preferences.Set("AppLanguage", languageCode);

                await Shell.Current.DisplayAlert(
                    "Idioma Alterado",
                    "O idioma será aplicado na próxima vez que você abrir o aplicativo.",
                    "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    $"Erro ao aplicar idioma: {ex.Message}",
                    "OK");
            }
        }

        [RelayCommand]
        private async Task ResetApp()
        {
            try
            {
                // Primeira confirmação
                bool confirm = await Shell.Current.DisplayAlert(
                    "⚠️ Confirmação",
                    "Tem certeza que deseja resetar o aplicativo?\n\n" +
                    "Esta ação irá:\n" +
                    "• Apagar todas as contas\n" +
                    "• Apagar todas as pastas\n" +
                    "• Limpar todas as configurações\n" +
                    "• Voltar ao tutorial inicial\n\n" +
                    "Esta ação NÃO pode ser desfeita!",
                    "Sim, resetar",
                    "Cancelar");

                if (!confirm) return;

                // Segunda confirmação (extra segurança)
                bool finalConfirm = await Shell.Current.DisplayAlert(
                    "🚨 Última Confirmação",
                    "ATENÇÃO: Você está prestes a apagar TODOS os seus dados!\n\n" +
                    "Esta é sua última chance de cancelar.",
                    "RESETAR TUDO",
                    "Cancelar");

                if (!finalConfirm) return;

                // Mostrar loading
                await Shell.Current.DisplayAlert(
                    "Processando...",
                    "Resetando aplicativo, aguarde...",
                    "OK");

                // Executar reset
                await PerformReset();

                // Sucesso
                await Shell.Current.DisplayAlert(
                    "✅ Reset Concluído",
                    "O aplicativo foi resetado com sucesso!\n\n" +
                    "Você será direcionado para o tutorial inicial.",
                    "OK");

                // Navegar para o tutorial
                await Shell.Current.GoToAsync("//TutorialPage1");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    $"Erro ao resetar aplicativo: {ex.Message}",
                    "OK");
            }
        }

        private async Task PerformReset()
        {
            try
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
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha no processo de reset: {ex.Message}");
            }
        }
    }
}