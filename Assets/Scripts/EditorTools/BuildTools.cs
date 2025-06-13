using System;
using System.IO;
using System.Linq;
using Common;
using JetBrains.Annotations;
using Sentry.Unity;
using UnityEditor;
using static System.String;

namespace EditorTools
{
    internal static class BuildTools
    {
        private const string BUILD_DIRECTORY_IOS = "Build_iOS";
        private const string BUILD_DIRECTORY_ANDROID = "Build_Android";

        //---------------------------------------------------------------------
        // Builders
        //---------------------------------------------------------------------

        [UsedImplicitly]
        private static void BuildFrever_iOS()
        {
            var bundleIdPrefix = GetArg("-bundleIdPrefix");
            var bundleIdSuffix = GetArg("-bundleIdSuffix");
            var productName = GetArg("-productName");
            var productVersion = GetArg("-productVersion");
            var buildNumber = GetArg("-buildNumber");
            var devBuildString = GetArg("-devBuild");
            var sentryEnabledString = GetArg("-sentryEnabled");
            var statsEnabledString = GetArg("-statsEnabled");
            var customDefineSymbols = GetArg("-defineSymbols") ?? Empty;

            if (!(IsNullOrEmpty(bundleIdPrefix) || IsNullOrEmpty(bundleIdSuffix)))
            {
                PlayerSettings.applicationIdentifier = $"{bundleIdPrefix}.{bundleIdSuffix}";
            }

            if (!IsNullOrEmpty(productName)) PlayerSettings.productName = productName;
            if (!IsNullOrEmpty(productVersion)) PlayerSettings.bundleVersion = productVersion;
            if (!IsNullOrEmpty(buildNumber)) PlayerSettings.iOS.buildNumber = buildNumber;

            ToggleSentryStateIfNeeded(sentryEnabledString);
            ToggleStatsMonitorIfNeeded(statsEnabledString, BuildTargetGroup.iOS);

            var isDevBuild = bool.TryParse(devBuildString, out var isDevBuildArg) && isDevBuildArg;

            if (isDevBuild)
            {
                EnableDebugLogs(BuildTargetGroup.iOS);
            }
            
            BuildPlayer_iOS(isDevBuild, customDefineSymbols);
        }

        [UsedImplicitly]
        private static void BuildFrever_Android()
        {
            var bundleIdPrefix = GetArg("-bundleIdPrefix");
            var bundleIdSuffix = GetArg("-bundleIdSuffix");
            var productName = GetArg("-productName");
            var productVersion = GetArg("-productVersion");
            var devBuildString = GetArg("-devBuild");
            var createSymbolsString = GetArg("-createSymbols");
            var sentryEnabledString = GetArg("-sentryEnabled");
            var statsEnabledString = GetArg("-statsEnabled");
            var keystorePath = GetArg("-keystorePath");
            var keystorePassword = GetArg("-keystorePassword");
            var keyaliasName = GetArg("-keyaliasName");
            var keyaliasPassword = GetArg("-keyaliasPassword");
            var customDefineSymbols = GetArg("-defineSymbols") ?? Empty;

            if (!(IsNullOrEmpty(bundleIdPrefix) || IsNullOrEmpty(bundleIdSuffix)))
            {
                PlayerSettings.applicationIdentifier = $"{bundleIdPrefix}.{bundleIdSuffix}";
            }

            if (!IsNullOrEmpty(productName)) PlayerSettings.productName = productName;
            if (!IsNullOrEmpty(productVersion)) PlayerSettings.bundleVersion = productVersion;
            
            SetCompilerConfiguration();

            if (!IsNullOrEmpty(createSymbolsString) && bool.TryParse(createSymbolsString, out var uploadSymbolsArg))
            {
                EditorUserBuildSettings.androidCreateSymbols = uploadSymbolsArg ? AndroidCreateSymbols.Public : AndroidCreateSymbols.Disabled;
            }

            ToggleSentryStateIfNeeded(sentryEnabledString);
            ToggleStatsMonitorIfNeeded(statsEnabledString, BuildTargetGroup.Android);

            if (!IsNullOrEmpty(keystorePath) && !IsNullOrEmpty(keystorePassword) &&
                !IsNullOrEmpty(keyaliasName) && !IsNullOrEmpty(keyaliasPassword))
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = keystorePath;
                PlayerSettings.Android.keystorePass = keystorePassword;
                PlayerSettings.Android.keyaliasName = keyaliasName;
                PlayerSettings.Android.keyaliasPass = keyaliasPassword;
            }

            var isDevBuild = bool.TryParse(devBuildString, out var isDevBuildArg) && isDevBuildArg;
            var filename = IsNullOrEmpty(bundleIdPrefix) ? "frever.apk" : $"{bundleIdSuffix}.apk";
            
            if (isDevBuild)
            {
                EnableDebugLogs(BuildTargetGroup.Android);
            }

            BuildPlayer_Android(isDevBuild, filename, customDefineSymbols);
        }
        
        [UsedImplicitly]
        private static void BuildFrever_AndroidRelease()
        {
            var bundleIdPrefix = GetArg("-bundleIdPrefix");
            var bundleIdSuffix = GetArg("-bundleIdSuffix");
            var productName = GetArg("-productName");
            var productVersion = GetArg("-productVersion");
            var bundleVersionCode = GetArg("-bundleVersionCode");
            var keystorePath = GetArg("-keystorePath");
            var keystorePassword = GetArg("-keystorePassword");
            var keyaliasName = GetArg("-keyaliasName");
            var keyaliasPassword = GetArg("-keyaliasPassword");
            var sentryEnabledString = GetArg("-sentryEnabled");
            var createSymbolsString = GetArg("-createSymbols");
            var customDefineSymbols = GetArg("-defineSymbols") ?? Empty;

            if (!(IsNullOrEmpty(bundleIdPrefix) || IsNullOrEmpty(bundleIdSuffix)))
            {
                PlayerSettings.applicationIdentifier = $"{bundleIdPrefix}.{bundleIdSuffix}";
            }

            if (!IsNullOrEmpty(productName)) PlayerSettings.productName = productName;
            if (!IsNullOrEmpty(productVersion)) PlayerSettings.bundleVersion = productVersion;
            if (!IsNullOrEmpty(bundleVersionCode) && int.TryParse(bundleVersionCode, out var bundleVersionCodeArg))
            {
                PlayerSettings.Android.bundleVersionCode = bundleVersionCodeArg;
            }
            
            // force release settings
            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Master);

            
            if (IsNullOrEmpty(keystorePath))
                throw new ArgumentNullException($"Keystore path is empty or not valid");

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = keyaliasName;
            PlayerSettings.Android.keyaliasPass = keyaliasPassword;
            
            ToggleSentryStateIfNeeded(sentryEnabledString);

            var filename = IsNullOrEmpty(bundleIdPrefix) ? "frever.aab" : $"{bundleIdSuffix}.aab";
            
            if (!IsNullOrEmpty(createSymbolsString) && bool.TryParse(createSymbolsString, out var uploadSymbolsArg))
            {
                EditorUserBuildSettings.androidCreateSymbols = uploadSymbolsArg ? AndroidCreateSymbols.Public : AndroidCreateSymbols.Disabled;;
            }
            
            BuildPlayer_Android(false, filename, customDefineSymbols);
        }

        [UsedImplicitly]
        private static void RefreshAssetDatabase()
        {
            AssetDatabase.Refresh();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void BuildPlayer_iOS(bool isDevBuild, string customDefineSymbols)
        {
            BuildPlayer(BuildTarget.iOS, BuildTargetGroup.iOS, BUILD_DIRECTORY_IOS, isDevBuild, customDefineSymbols);
        }

        private static void BuildPlayer_Android(bool isDevBuild, string fileName, string customDefineSymbols)
        {
            BuildPlayer(BuildTarget.Android, BuildTargetGroup.Android, $"{BUILD_DIRECTORY_ANDROID}/{fileName}", isDevBuild, customDefineSymbols);
        }

        private static void BuildPlayer(BuildTarget target, BuildTargetGroup targetGroup, string buildDir, bool isDevBuild,
            string customDefineSymbols)
        {
            try
            {
                LoggingControl.EnableLogging(isDevBuild);

                var buildOptions = BuildOptions.None;

                if (isDevBuild)
                {
                    buildOptions |= BuildOptions.Development;
                }

                if (target == BuildTarget.iOS && Directory.Exists(buildDir))
                {
                    buildOptions |= BuildOptions.AcceptExternalModificationsToPlayer;
                }

                var defaultDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup) ?? Empty;
                UnityEngine.Debug.Log($"Default define symbols: {defaultDefineSymbols}");

                var delimiter = defaultDefineSymbols.EndsWith(";") ? Empty : ";";
                var buildDefineSymbols = $"{defaultDefineSymbols}{delimiter}{customDefineSymbols}";
                UnityEngine.Debug.Log($"Build define symbols: {buildDefineSymbols}");

                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, buildDefineSymbols);

                BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = GetAllEditorBuildScenePaths(),
                    options = buildOptions,
                    target = target,
                    targetGroup = targetGroup,
                    locationPathName = buildDir
                });

                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defaultDefineSymbols);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static string GetArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
        
        private static void ToggleSentryStateIfNeeded(string sentryEnabledString)
        {
            if (IsNullOrEmpty(sentryEnabledString) ||
                !bool.TryParse(sentryEnabledString, out var sentryEnabled)) return;
            
            var sentryOptions =
                AssetDatabase.LoadAssetAtPath<ScriptableSentryUnityOptions>(
                    Constants.FileDefaultPaths.SENTRY_OPTIONS_PATH);

            if (!sentryOptions) return;

            sentryOptions.Enabled = sentryEnabled;
            EditorUtility.SetDirty(sentryOptions);
        }
        
        private static void ToggleStatsMonitorIfNeeded(string statsEnabledString, BuildTargetGroup buildTargetGroup)
        {
            if (IsNullOrEmpty(statsEnabledString) ||
                !bool.TryParse(statsEnabledString, out var statsEnabled)) return;
            
            if (!statsEnabled) return;
                
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var delimiter = defines.EndsWith(";") ? Empty : ";";
            defines = $"{defines}{delimiter}STATS_ENABLED";
                    
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }
        
        private static void EnableDebugLogs(BuildTargetGroup buildTargetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            
            if (defines.Contains(Debug.ENABLE_CONDITION)) return;
            
            var delimiter = defines.EndsWith(";") ? Empty : ";";
            defines = $"{defines}{delimiter}{Debug.ENABLE_CONDITION}";
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }

        private static void SetCompilerConfiguration()
        {
            var il2CPPCompilerConfigArg = GetArg("-il2cppCompilerConfig");
            var il2CPPConfig = Il2CppCompilerConfiguration.Release;
            
            if (!IsNullOrEmpty(il2CPPCompilerConfigArg))
            {
                switch (il2CPPCompilerConfigArg)
                {
                    case "Debug":
                        il2CPPConfig = Il2CppCompilerConfiguration.Debug;
                        break;
                    case "Master":
                        il2CPPConfig = Il2CppCompilerConfiguration.Master;
                        break;
                }
            }

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, il2CPPConfig);
        }

        private static string[] GetAllEditorBuildScenePaths(bool enabled = true)
        {
            return EditorBuildSettings.scenes
                                      .Where(scene => scene.enabled == enabled)
                                      .Select(scene => scene.path)
                                      .ToArray();
        }
    }
}