using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBagBunker.Helper
{
    public class LocalizationHelper
    {
        private readonly Dictionary<string, Dictionary<string, string>> _translations;

        public LocalizationHelper(string jsonFilePath)
        {
            var json = File.ReadAllText(jsonFilePath);
            _translations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
        }

        public string GetTranslation(string key, string language)
        {

            if (_translations.ContainsKey(language.ToLower()) && _translations[language.ToLower()].ContainsKey(key))
            {
                return _translations[language.ToLower()][key];
            }

            return key;
        }
    }
}
