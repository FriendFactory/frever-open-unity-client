using JetBrains.Annotations;
using QFSW.QC;

namespace Utils
{
    [UsedImplicitly]
    public static class AppIconUtils
    {
        [Command("set-app-icon")]
        public static void SetAppIcon(string name)
        {
            #if UNITY_IOS
            AppIconChanger.iOS.SetAlternateIconName(name);
            #else
            UnityEngine.Debug.LogError($"Changing icon is not supported for current platform");
            #endif
        }
    }
}
