using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS || UNITY_TVOS
using UnityEditor.iOS.Xcode;
#endif

namespace EditorTools
{
    public static partial class BuildPostProcessor
    {
        private const string MIN_IOS_DEPLOYMENT_TARGET = "11.0";
        private const string IOS_DEV_TEAM_ID = "MANG8XFF34";

        private const string UNITY_IPHONE_FILE_INJECTION_CONFIG_PATH = "Assets/Editor/BuildTools/PostBuildFileInjectionConfig.asset";
        private const string PHOTO_LIBRARY_USAGE_DESCRIPTION = "Save media to Photos";

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        // must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40)
        // and that it's added before "pod install" (50)
        [PostProcessBuild(45)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            #if UNITY_IOS

            ModifyPBXProject(path);
            ModifyPBXProjectAsText(path);
            ModifyPList(path);
            FixPodFiles(path);

            #endif
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        #if UNITY_IOS

        private static void ModifyPBXProject(string path)
        {
            // Read
            var projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));

            // Modify
            var mainTargetGuid = project.GetUnityMainTargetGuid();
            project.SetBuildProperty(mainTargetGuid, "ENABLE_BITCODE", "NO");
            project.AddBuildProperty(mainTargetGuid, "OTHER_LDFLAGS", "-framework Photos");
            project.AddBuildProperty(mainTargetGuid, "OTHER_LDFLAGS", "-framework MobileCoreServices");
            project.AddBuildProperty(mainTargetGuid, "OTHER_LDFLAGS", "-framework ImageIO");
            project.AddBuildProperty(mainTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            project.AddBuildProperty(mainTargetGuid, @"CLANG\ENABLE_MODULE", "YES");

            var frameworkGuid = project.GetUnityFrameworkTargetGuid();
            project.AddFrameworkToProject(frameworkGuid, "Photos.framework", false);
            project.AddFrameworkToProject(frameworkGuid, "MobileCoreServices.framework", false);
            project.AddFrameworkToProject(frameworkGuid, "ImageIO.framework", false);

#if !UNITY_2021_3_OR_NEWER // DWC-2021.3.18 upgrade: MapFileParser.sh no longer used in Unity 2021.3
            DWC-2021.3.18 upgrade: MapFileParser.sh no longer used in Unity 2021.3
            const string shellPath = "/bin/sh";
            const int index = 0;
            const string name = "Fix Permissions in Windows Generated XcodeProject";
            const string shellScript = "chmod a+x \"$PROJECT_DIR/MapFileParser.sh\"";
            project.InsertShellScriptBuildPhase(index, frameworkGuid, name, shellPath, shellScript);
#endif

            InjectCustomFiles(path, project, UNITY_IPHONE_FILE_INJECTION_CONFIG_PATH);

            // Write
            File.WriteAllText(projectPath, project.WriteToString());
        }

        private static void InjectCustomFiles(string projectPath, PBXProject pbxProject, string injectFileConfigPath)
        {
            var fileInjectionConfig = AssetDatabase.LoadAssetAtPath<PostBuildFileInjectionConfig>(injectFileConfigPath);

            if(!fileInjectionConfig) return;


            foreach (var fileConfig in fileInjectionConfig.Entries)
            {
                var filePath = Path.Combine(projectPath, fileConfig.ProjectPath);
                var fileExists = File.Exists(filePath);

                if (fileExists) File.Delete(filePath);
                Debug.Log("BuiildPostProcess: " + fileConfig.AssetFilePath + " " + filePath);
                if (fileConfig.AssetFilePath != "") // DWC-2022 upgrade. why is this fileConfig.AssetFilePath = "", it causes an exception below  seems to be from the PostBuildFileInjectionsConfig.asset
                {
                    File.Copy(fileConfig.AssetFilePath, filePath);
                    pbxProject.AddFile(fileConfig.ProjectPath, fileConfig.ProjectPath);
                }
                else
                {
                    Debug.LogWarning("BuildPostProcess: fileConfig.AssetFilePath is empty for InjectCustomFiles: " + fileConfig.AssetFilePath + ", " + filePath);
                }
            }
        }

        private static void ModifyPBXProjectAsText(string path)
        {
            // Read
            var projectPath = PBXProject.GetPBXProjectPath(path);
            var projectInString = File.ReadAllText(projectPath);
            // Modify
            projectInString = projectInString.Replace("ENABLE_BITCODE = YES;", $"ENABLE_BITCODE = NO;");
            // Write
            File.WriteAllText(projectPath, projectInString);
        }

        private static void ModifyPList(string path)
        {
            var plistPath = Path.Combine(path, "Info.plist");

            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            var rootDict = plist.root;

            rootDict.SetString("MinimumOSVersion", PlayerSettings.iOS.targetOSVersionString);
            rootDict.SetString("NSPhotoLibraryUsageDescription", PHOTO_LIBRARY_USAGE_DESCRIPTION);
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", PHOTO_LIBRARY_USAGE_DESCRIPTION);
            rootDict.SetBoolean("LSSupportsOpeningDocumentsInPlace", false);
            rootDict.SetBoolean("UIFileSharingEnabled", false);
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            File.WriteAllText(plistPath, plist.WriteToString());
        }

        private static void FixPodFiles(string buildPath)
        {
            using (var sw = File.AppendText( buildPath + "/Podfile" ))
            {
                sw.WriteLine($"min_ios_deployment_target = Gem::Version.new(\"{MIN_IOS_DEPLOYMENT_TARGET}\")");

                sw.WriteLine("post_install do |installer|");
                    sw.WriteLine("installer.generated_projects.each do |project|");
                        sw.WriteLine("project.targets.each do |target|");
                            sw.WriteLine("target.build_configurations.each do |config|");

                            // Force development team for pods
                            sw.WriteLine($"config.build_settings[\"DEVELOPMENT_TEAM\"] = \"{IOS_DEV_TEAM_ID}\"");

                            // Force iOS version for pods
                            sw.WriteLine("pod_ios_deployment_target = Gem::Version.new(config.build_settings[\'IPHONEOS_DEPLOYMENT_TARGET\'])");
                            sw.WriteLine("if pod_ios_deployment_target <= min_ios_deployment_target");
                                sw.WriteLine("config.build_settings.delete \'IPHONEOS_DEPLOYMENT_TARGET\'");
                            sw.WriteLine("end");

                sw.WriteLine("end\nend\nend\nend");
            }
        }

        #endif
    }
}