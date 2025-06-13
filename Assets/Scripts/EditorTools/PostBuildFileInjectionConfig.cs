using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(menuName = "ScriptableObject/PostBuildFileInjectionConfig", fileName = "PostBuildFileInjectionConfig")]
internal class PostBuildFileInjectionConfig : ScriptableObject
{
    [Serializable]
    internal class FileInjectionConfigEntry
    {
        [SerializeField]
        private Object _file;
        [SerializeField]
        private string _projectPath;

        public string AssetFilePath => AssetDatabase.GetAssetPath(_file);
        public string ProjectPath => Path.Combine(_projectPath, Path.GetFileName(AssetFilePath));
    }
    
    public FileInjectionConfigEntry[] Entries;
}