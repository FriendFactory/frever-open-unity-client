using UnityEditor;
using UnityEditor.Build;

namespace EditorTools
{
    /// <summary>
    /// Fix for unity switching platform problem(Jira ticket: FREV-8525)
    /// If developer switches the platform from Standalone OSX to iOS/Android, he gets few broken prefabs
    /// That can be fixed by forcing reimporting of prefabs and scenes, what we do here.
    /// </summary>
    internal sealed class ForceReimportOnPlatformChange : IActiveBuildTargetChanged
    {
        private static readonly string[] FOLDERS_TO_REIMPORT = {"Prefabs", "Scenes", "Resources"};

        public int callbackOrder => 0;
        
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            foreach (var folder in FOLDERS_TO_REIMPORT)
            {
                AssetDatabase.ImportAsset($"Assets/{folder}" , ImportAssetOptions.ImportRecursive | ImportAssetOptions.DontDownloadFromCacheServer);
            }
        }
    }
}