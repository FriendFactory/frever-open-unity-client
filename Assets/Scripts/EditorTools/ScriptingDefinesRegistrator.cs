using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    public class ScriptingDefinesRegistrator : MonoBehaviour
    {
        #if FEED_SHOW_VIDEO_IDS
        [MenuItem("Tools/Friend Factory/Defines/Feed/Hide Video IDs")]
        #else
        [MenuItem("Tools/Friend Factory/Defines/Feed/Show Video IDs")]
        #endif
        private static void ShowFeedVideoIds()
        {
            var currentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentTarget);

            #if FEED_SHOW_VIDEO_IDS
                defineSymbols = defineSymbols.Replace("FEED_SHOW_VIDEO_IDS", string.Empty);
            #else
                defineSymbols += ";FEED_SHOW_VIDEO_IDS";
            #endif

            defineSymbols = defineSymbols.Replace(";;", ";");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentTarget, defineSymbols);
        }
    }
}