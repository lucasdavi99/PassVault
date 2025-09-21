using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.Interfaces
{
    public interface ILocalizationService
    {

        
        string GetString(string key);
       
        string GetString(string key, params object[] args);

        Task SetLanguageAsync(string languageCode);

        
        string CurrentLanguage { get; }

      
        event EventHandler LanguageChanged;
    }
}
