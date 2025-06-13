using I2.Loc;
using UnityEditor;

namespace EditorTools
{
    [InitializeOnLoad]
    public static class LocalizationCleaner
    {
        //---------------------------------------------------------------------
        // Static
        //---------------------------------------------------------------------

        static LocalizationCleaner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode) return;

            var source = LocalizationManager.Sources[LocalizationManager.REMOTE_SOURCE];

            source.mTerms.Clear();
            source.mDictionary.Clear();
            source.mAssetDictionary.Clear();

            source.Editor_SetDirty();
        }
    }
}