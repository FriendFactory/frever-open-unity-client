using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Bridge;
using I2.Loc;
using UnityEngine;

namespace Modules.AssetsStoraging.Core
{
    public sealed class LocalizationSetup
    {
        private readonly IBridge _bridge;
        public bool IsLocalizationDataSetup { get; private set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public LocalizationSetup(IBridge serverBridge)
        {
            _bridge = serverBridge;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task<bool> FetchLocalization(bool autoApply = true)
        {
            var iso = GetIsoFromSystemLanguage();
            var result = await _bridge.GetLocalizationData(iso);

            var locFilePath = result.LastVersionFilePath;

            if (result.IsError)
            {
                Debug.LogError($"Failed to fetch localization data. Reason: {result.ErrorMessage}");
                locFilePath = result.LastCachedFilePath;
            }

            if (string.IsNullOrEmpty(locFilePath))
            {
                Debug.LogError("Localization file path is empty.");
                return false;
            }

            if (autoApply && !IsLocalizationDataSetup)
            {
                ImportCSV(locFilePath);
                IsLocalizationDataSetup = true;
            }

            return true;
        }

        public void ApplyLastCacheLocalizationDataImmediate()
        {
            if (IsLocalizationDataSetup || !_bridge.HasCached(GetIsoFromSystemLanguage(), out var filePath)) return;
            
            ImportCSV(filePath);
            IsLocalizationDataSetup = true;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void ImportCSV(string path, eSpreadsheetUpdateMode updateMode = eSpreadsheetUpdateMode.Merge)
        {
            if (!LocalizationManager.UpdateSources())
            {
                Debug.LogError("Failed to update localization sources.");
                return;
            }

            var source = LocalizationManager.Sources[LocalizationManager.REMOTE_SOURCE];
            var csvText = LocalizationReader.ReadCSVfile(path, Encoding.UTF8);
            var error = source.Import_CSV(string.Empty, csvText, updateMode, ',');

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Failed to import localization data. Reason: {error}");
            }

            LocalizationManager.LocalizeAll();
        }

        [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
        private static string GetIsoFromSystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:    return "eng";
                case SystemLanguage.Spanish:    return "spa";
                case SystemLanguage.German:     return "deu";
                case SystemLanguage.French:     return "fra";
                case SystemLanguage.Portuguese: return "por";

                default:                        return "eng";
            }
        }
    }
}