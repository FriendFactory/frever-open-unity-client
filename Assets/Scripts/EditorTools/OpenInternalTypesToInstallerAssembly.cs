using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorTools
{
    /// <summary>
    /// That script is responsible for creation simple .cs file with assembly attribute [InternalVisisbleTo]
    /// to open access of internal types and methods to Installer assembly.
    /// It is necessary for binding internal realization to public interfaces
    /// </summary>
    [CreateAssetMenu]
    public class OpenInternalTypesToInstallerAssembly : Editor
    {
        private const string UNITY_INTERNAL_GET_PATH_METHOD = "GetActiveFolderPath";
        private const string FILE_NAME = "InternalVisibilitySetting.cs";
        private const string ASSEMBLY_EXTENSION = "asmdef";
        private const string ROOT_ASSETS_FOLDER = "Assets";

        private const string FILE_CONTENT =
            "// It provides access of internal objects from Installer assembly \n" +
            "// and we can use that for accessing from Test assembly, if we will have it \n" +
            "\n" +
            "using System.Runtime.CompilerServices;\n" +
            "\n" +
            "[assembly: InternalsVisibleTo(\"Installers\")]"; 
        
        [MenuItem("Assets/Create/Friend Factory/Internal Type Access Provider", priority = 10)]
        public static void CreateAccessProviderFile()
        {
            var folderPath = GetCurrentFolderFullPath();

            var isJustRootFolder = folderPath == Application.dataPath;
            if (isJustRootFolder)
            {
                EditorUtility.DisplayDialog("Error", "You can't create it in root Assets folder", "Ok");
                return;
            }
            
            var filePath = Path.Combine(folderPath, FILE_NAME);

            if (!IsFolderValidForAction(folderPath))
            {
                EditorUtility.DisplayDialog("Error", "Folder must contain assembly definition file", "Ok");
                return;
            }
            
            if (File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("Error", $"File {FILE_NAME} is already existed", "Ok");
                return;
            }

            GenerateAccessProviderFile(folderPath);
            AssetDatabase.Refresh();
       
            var localPath = Path.Combine(folderPath, FILE_NAME);
            SetIcon(localPath);
            
            AssetDatabase.Refresh();
        }

        private static void SetIcon(string relativeAssetPath)
        {
            var metaFile = relativeAssetPath + ".meta";
            var content = File.ReadAllText(metaFile);
            content = content.Replace("icon: {instanceID: 0}",
                "icon: {fileID: 2800000, guid: d2da0f992a4f447fca562888537ec033, type: 3}");
            File.WriteAllText(metaFile,content);
        }

        private static string GetCurrentFolderFullPath()
        {
            return Path.Combine(Application.dataPath, GetCurrentFolderRelativePath());
        }
        
        private static string GetCurrentFolderRelativePath()
        {
            var projectWindowUtilType = typeof(ProjectWindowUtil);
            var getActiveFolderPath = projectWindowUtilType.GetMethod(UNITY_INTERNAL_GET_PATH_METHOD, BindingFlags.Static | BindingFlags.NonPublic);
            var localPathWithAssetsFolderRoot = getActiveFolderPath.Invoke(null, new object[0]).ToString();

            var isJustRootFolder = localPathWithAssetsFolderRoot.Length == ROOT_ASSETS_FOLDER.Length;
            if (isJustRootFolder)
                return string.Empty;
            
            var rootFolderNameLength = (ROOT_ASSETS_FOLDER + '/').Length;
            return localPathWithAssetsFolderRoot.Substring(rootFolderNameLength);
        }

        private static bool IsFolderValidForAction(string folderPath)
        {
            var assemblyDefinition = Directory.GetFiles(folderPath, $"*.{ASSEMBLY_EXTENSION}");
            return assemblyDefinition.Length == 1;
        }

        private static void GenerateAccessProviderFile(string folderPath)
        {
            var filePath = Path.Combine(folderPath, FILE_NAME);
            File.WriteAllText(filePath,FILE_CONTENT);
        }
    }
}
