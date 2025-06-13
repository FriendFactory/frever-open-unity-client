using Common;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EditorTools
{
    public class PlayerSettingsBuildPreprocessor: IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string PREF_KEY_PREPROCESSOR_ENABLED = "PlayerSettingsBuildPreprocessor.Enabled";

        private static string _productNameBackup;
        private static string _appIdentifierBackup;
        private static string _bundleVersionBackup;
        private static string _buildNumberBackup;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!EditorPrefs.GetBool(PREF_KEY_PREPROCESSOR_ENABLED, false)) return;

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            _productNameBackup = PlayerSettings.productName;
            _appIdentifierBackup = PlayerSettings.applicationIdentifier;
            _bundleVersionBackup = PlayerSettings.bundleVersion;
            _buildNumberBackup = PlayerSettings.iOS.buildNumber;

            var repoInfo = GitRepoInfo.GetRepoInfo();
            var dateTime = System.DateTime.Now.ToString("yyMMddHHmm");
            var dateTimeSuffix = $".{dateTime}";

            PlayerSettings.productName = GetBranchName(repoInfo, true) + dateTimeSuffix;
            PlayerSettings.applicationIdentifier = $"com.friendfactory.{GetBranchName(repoInfo)}.{dateTime}";
            PlayerSettings.bundleVersion += dateTimeSuffix;
            PlayerSettings.iOS.buildNumber = dateTime;
            
            var runtimeBuildInfo = Resources.Load<RuntimeBuildInfo>("ScriptableObjects/RuntimeBuildInfo");
            runtimeBuildInfo.SetBuildNumber(_buildNumberBackup);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (string.IsNullOrEmpty(_productNameBackup)) return;

            PlayerSettings.productName = _productNameBackup;
            PlayerSettings.applicationIdentifier = _appIdentifierBackup;
            PlayerSettings.bundleVersion = _bundleVersionBackup;
            PlayerSettings.iOS.buildNumber = _buildNumberBackup;

            _productNameBackup = null;
            _appIdentifierBackup = null;
            _bundleVersionBackup = null;
            _buildNumberBackup = null;
        }

        public int callbackOrder { get; }

        //---------------------------------------------------------------------
        // Menu
        //---------------------------------------------------------------------

        [MenuItem("Tools/Friend Factory/Build/Toggle Player Settings Preprocessing")]
        private static void TogglePlayerSettingsPreprocessing()
        {
            var currentState = !EditorPrefs.GetBool(PREF_KEY_PREPROCESSOR_ENABLED, false);
            EditorPrefs.SetBool(PREF_KEY_PREPROCESSOR_ENABLED, currentState);

            UnityEngine.Debug.Log("Player Settings preprocessing " + ((currentState) ? "<color=green>enabled</color>" : "<color=red>disabled</color>"));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static string GetBranchName(GitRepoInfo repoInfo, bool uppercaseFirst = false)
        {
            var name = repoInfo.BranchName.Substring(repoInfo.BranchName.LastIndexOf('/') + 1);
            return (uppercaseFirst) ? UppercaseFirst(name) : name;
        }

        private static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
