using OrionVault.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrionVault.Services
{
    internal class LocalizationService : ILocalizationService
    {
        private readonly ConcurrentDictionary<string, Dictionary<string, object>> _translations = new();
        private string _currentLanguage;
        private Dictionary<string, object> _currentTranslations;

        public string CurrentLanguage => _currentLanguage;
        public event EventHandler LanguageChanged;

        public LocalizationService()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // 1. Tenta carregar o idioma salvo pelo usuário
                var savedLanguage = Preferences.Get("AppLanguage", string.Empty);

                if (!string.IsNullOrEmpty(savedLanguage))
                {
                    await SetLanguageAsync(savedLanguage);
                }
                else
                {
                    // 2. Se não houver idioma salvo, detecta o do dispositivo
                    var deviceLanguage = CultureInfo.CurrentUICulture.Name; // ex: "en-US", "pt-BR"

                    // 3. Define pt-BR se for o idioma do dispositivo, caso contrário, o padrão será en-US
                    var initialLanguage = deviceLanguage == "pt-BR" ? "pt-BR" : "en-US";
                    await SetLanguageAsync(initialLanguage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro na inicialização da localização: {ex.Message}");
                // Fallback final para inglês em caso de erro inesperado
                await SetLanguageAsync("en-US");
            }
        }

        public async Task SetLanguageAsync(string languageCode)
        {
            try
            {
                if (_translations.TryGetValue(languageCode, out var cachedTranslations))
                {
                    _currentLanguage = languageCode;
                    _currentTranslations = cachedTranslations;
                }
                else
                {
                    var translations = await LoadLanguageFileAsync(languageCode);
                    if (translations != null)
                    {
                        _translations[languageCode] = translations;
                        _currentLanguage = languageCode;
                        _currentTranslations = translations;
                    }
                    else
                    {
                        // Lógica de Fallback: Se o idioma não for encontrado, tenta o inglês.
                        System.Diagnostics.Debug.WriteLine($"Arquivo de tradução para '{languageCode}' não encontrado.");
                        if (languageCode != "en-US")
                        {
                            System.Diagnostics.Debug.WriteLine("Tentando fallback para 'en-US'.");
                            await SetLanguageAsync("en-US");
                        }
                        else if (languageCode != "pt-BR")
                        {
                            // Se o inglês (padrão) também falhar, tenta o pt-BR como último recurso
                            System.Diagnostics.Debug.WriteLine("Fallback para 'en-US' falhou. Tentando 'pt-BR'.");
                            await SetLanguageAsync("pt-BR");
                        }
                        else
                        {
                            // Se tudo falhar, carrega traduções básicas
                            System.Diagnostics.Debug.WriteLine("Todos os fallbacks falharam. Carregando traduções básicas.");
                            _currentLanguage = "pt-BR";
                            _currentTranslations = GetBasicTranslations();
                        }
                        return; // Evita salvar a preferência de um idioma que falhou em carregar
                    }
                }

                // Salva a preferência do idioma carregado com sucesso
                Preferences.Set("AppLanguage", languageCode);

                // Notifica que o idioma mudou
                LanguageChanged?.Invoke(this, EventArgs.Empty);

                System.Diagnostics.Debug.WriteLine($"Idioma alterado para: {languageCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao definir idioma {languageCode}: {ex.Message}");

                // Fallback de exceção para inglês
                if (languageCode != "en-US")
                {
                    await SetLanguageAsync("en-US");
                }
            }
        }

        private async Task<Dictionary<string, object>> LoadLanguageFileAsync(string languageCode)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"OrionVault.Resources.Languages.{languageCode}.json";

                System.Diagnostics.Debug.WriteLine($"Tentando carregar recurso: {resourceName}");

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Recurso não encontrado: {resourceName}");

                    // Listar todos os recursos disponíveis para debug
                    var availableResources = assembly.GetManifestResourceNames();
                    System.Diagnostics.Debug.WriteLine($"Recursos disponíveis: {string.Join(", ", availableResources)}");

                    return null;
                }

                using var reader = new StreamReader(stream);
                var jsonContent = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent, options);
                System.Diagnostics.Debug.WriteLine($"Arquivo de idioma {languageCode} carregado com sucesso. Chaves: {result?.Count ?? 0}");

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar arquivo de idioma {languageCode}: {ex.Message}");
                return null;
            }
        }

        private Dictionary<string, object> GetBasicTranslations()
        {
            // Traduções básicas de fallback
            return new Dictionary<string, object>
            {
                ["common"] = new Dictionary<string, object>
                {
                    ["ok"] = "OK",
                    ["cancel"] = "Cancel",
                    ["error"] = "Error",
                    ["success"] = "Success"
                },
                ["settings"] = new Dictionary<string, object>
                {
                    ["title"] = "Settings",
                    ["language"] = new Dictionary<string, object>
                    {
                        ["changed"] = "Language Changed",
                        ["restart_message"] = "The language will be applied the next time you open the app."
                    }
                }
            };
        }

        public string GetString(string key)
        {
            if (_currentTranslations == null || string.IsNullOrEmpty(key))
                return key;

            try
            {
                var keys = key.Split('.');
                object current = _currentTranslations;

                foreach (var keyPart in keys)
                {
                    if (current is Dictionary<string, object> dict && dict.TryGetValue(keyPart, out var value))
                    {
                        if (value is JsonElement jsonElement)
                        {
                            if (jsonElement.ValueKind == JsonValueKind.String)
                                return jsonElement.GetString();

                            if (jsonElement.ValueKind == JsonValueKind.Object)
                            {
                                current = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.GetRawText());
                                continue;
                            }
                        }
                        else if (value is string stringValue)
                        {
                            return stringValue;
                        }
                        else
                        {
                            current = value;
                            continue;
                        }
                    }
                    else
                    {
                        // Chave não encontrada, retornar a chave original
                        System.Diagnostics.Debug.WriteLine($"Chave de tradução não encontrada: {key}");
                        return key;
                    }
                }

                return key;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar tradução para '{key}': {ex.Message}");
                return key;
            }
        }

        public string GetString(string key, params object[] args)
        {
            var text = GetString(key);

            try
            {
                return string.Format(text, args);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao formatar string '{text}' com argumentos: {ex.Message}");
                return text;
            }
        }
    }

    /// <summary>
    /// Classe estática para acesso global às traduções
    /// </summary>
    public static class L
    {
        private static ILocalizationService _localizationService;

        /// <summary>
        /// Inicializa o serviço de localização global
        /// </summary>
        public static void Initialize(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Obtém uma tradução pela chave
        /// </summary>
        public static string Text(string key) => _localizationService?.GetString(key) ?? key;

        /// <summary>
        /// Obtém uma tradução formatada pela chave
        /// </summary>
        public static string Text(string key, params object[] args) => _localizationService?.GetString(key, args) ?? key;
    }
}