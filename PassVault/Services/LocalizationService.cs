using PassVault.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PassVault.Services
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
            InitializeAsync().ConfigureAwait(false);
        }

        private async Task InitializeAsync()
        {
            // Carregar idioma salvo ou usar padrão
            var savedLanguage = Preferences.Get("AppLanguage", "pt-BR");
            await SetLanguageAsync(savedLanguage);
        }

        public async Task SetLanguageAsync(string languageCode)
        {
            try
            {
                if (_translations.ContainsKey(languageCode))
                {
                    _currentLanguage = languageCode;
                    _currentTranslations = _translations[languageCode];
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
                        // Fallback para português se não conseguir carregar
                        if (languageCode != "pt-BR")
                        {
                            await SetLanguageAsync("pt-BR");
                        }
                        return;
                    }
                }

                // Salvar a preferência
                Preferences.Set("AppLanguage", languageCode);

                // Notificar que o idioma mudou
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao definir idioma {languageCode}: {ex.Message}");

                // Fallback para português
                if (languageCode != "pt-BR")
                {
                    await SetLanguageAsync("pt-BR");
                }
            }
        }

        private async Task<Dictionary<string, object>> LoadLanguageFileAsync(string languageCode)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"PassVault.Resources.Languages.{languageCode}.json";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    // Tentar carregar do sistema de arquivos como fallback
                    var filePath = Path.Combine(FileSystem.AppDataDirectory, "Languages", $"{languageCode}.json");
                    if (File.Exists(filePath))
                    {
                        var fileContent = await File.ReadAllTextAsync(filePath);
                        return JsonSerializer.Deserialize<Dictionary<string, object>>(fileContent);
                    }
                    return null;
                }

                using var reader = new StreamReader(stream);
                var jsonContent = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent, options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar arquivo de idioma {languageCode}: {ex.Message}");
                return null;
            }
        }

        public string GetString(string key)
        {
            if (_currentTranslations == null || string.IsNullOrEmpty(key))
                return key;

            try
            {
                var keys = key.Split('.');
                var current = _currentTranslations;

                foreach (var keyPart in keys)
                {
                    if (current.TryGetValue(keyPart, out var value))
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
                        else if (value is Dictionary<string, object> dictValue)
                        {
                            current = dictValue;
                            continue;
                        }
                    }
                    else
                    {
                        // Chave não encontrada, retornar a chave original
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
