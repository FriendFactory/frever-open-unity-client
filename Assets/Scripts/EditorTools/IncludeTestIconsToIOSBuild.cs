#if UNITY_IOS
using System.IO;
using System.Linq;
using AppIconChanger.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace EditorTools
{
    public static class IncludeTestIconsToIOSBuild
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            var alternateIcons = AssetDatabase.FindAssets($"t:{nameof(AlternateIcon)}")
                                              .Select(AssetDatabase.GUIDToAssetPath)
                                              .Select(AssetDatabase.LoadAssetAtPath<AlternateIcon>)
                                              .ToArray();
            if (alternateIcons.Length == 0) return;
            
            var pbxProjectPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj", "project.pbxproj");
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);

            var targetGuid = pbxProject.GetUnityMainTargetGuid();
            pbxProject.SetBuildProperty(targetGuid, "ASSETCATALOG_COMPILER_INCLUDE_ALL_APPICON_ASSETS", true.ToString());

            pbxProject.WriteToFile(pbxProjectPath);
        }
    }
}
#endif
