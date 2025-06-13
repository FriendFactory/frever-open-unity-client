using UnityEditor;
using UnityEngine;

namespace Utils
{
    public static class PlatformUtils
    {
        public static RuntimePlatform GetRuntimePlatform()
        {
        #if UNITY_EDITOR
            return EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS
                ? RuntimePlatform.IPhonePlayer
                : RuntimePlatform.Android;
        #else
            return Application.platform;
        #endif
        }
    }
}