using UnityEditor;

namespace EditorTools
{
    public static class LoggingControl
    {
        private const string FREVER_LOG_ENABLE = Debug.ENABLE_CONDITION;
        private const string AVPRO_LOG_DISABLE = "AVPROVIDEO_DISABLE_LOGGING";
        
        private static string ScriptingDefineSymbols
        {
            get => PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
            set => PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, value);
        }

        public static bool IsLoggingEnable => IsFreverLogsEnabled && IsAVProLogsEnabled;

        private static bool IsFreverLogsEnabled => ScriptingDefineSymbols.Contains(FREVER_LOG_ENABLE);
        private static bool IsAVProLogsEnabled =>  !ScriptingDefineSymbols.Contains(AVPRO_LOG_DISABLE);
        
        public static void EnableLogging(bool enable)
        {
            var appDefineSymbols = ScriptingDefineSymbols;
            if (enable)
            {
                AppendToDefineSymbols(ref appDefineSymbols, FREVER_LOG_ENABLE);
                RemoveFromDefineSymbols(ref appDefineSymbols, AVPRO_LOG_DISABLE);
            }
            else
            {
                AppendToDefineSymbols(ref appDefineSymbols, AVPRO_LOG_DISABLE);
                RemoveFromDefineSymbols(ref appDefineSymbols, FREVER_LOG_ENABLE);
            }
            
            appDefineSymbols = appDefineSymbols.Replace(";;", ";");
            ScriptingDefineSymbols = appDefineSymbols;
        }

        private static void AppendToDefineSymbols(ref string defineSymbols, string appendKey)
        {
            if(defineSymbols.Contains(appendKey)) return;
            defineSymbols += $";{appendKey}";
        }
        
        private static void RemoveFromDefineSymbols(ref string defineSymbols, string removeKey)
        {
            if(!defineSymbols.Contains(removeKey)) return;
            defineSymbols = defineSymbols.Replace(removeKey, string.Empty);
        }
    }
}