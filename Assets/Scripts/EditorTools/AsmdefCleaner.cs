using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EditorTools
{
    public class AsmdefCleaner : MonoBehaviour
    {
        private const string ASMDEF_TYPE_FILTER = "t: AssemblyDefinitionAsset";
        private const string TARGET_FOLDER = "Assets";

        private static readonly FieldInfo NAME_FIELD;
        private static readonly FieldInfo REFS_FIELD;
        private static readonly MethodInfo FROM_JSON_METHOD;
        private static readonly MethodInfo TO_JSON_METHOD;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        static AsmdefCleaner()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var assemblyDataType = assembly.GetType("UnityEditor.Scripting.ScriptCompilation.CustomScriptAssemblyData");

            NAME_FIELD = assemblyDataType.GetField("name", BindingFlags.Instance | BindingFlags.Public);
            REFS_FIELD = assemblyDataType.GetField("references", BindingFlags.Instance | BindingFlags.Public);

            FROM_JSON_METHOD = assemblyDataType.GetMethod("FromJson", BindingFlags.Static | BindingFlags.Public);
            TO_JSON_METHOD = assemblyDataType.GetMethod("ToJson", BindingFlags.Static | BindingFlags.Public);
        }

        //---------------------------------------------------------------------
        // Menu
        //---------------------------------------------------------------------

        [MenuItem("Tools/Friend Factory/Clean up *.asmdef references")]
        private static void UpdateAssemblies()
        {
            var assets = AssetDatabase.FindAssets(ASMDEF_TYPE_FILTER, new[] {TARGET_FOLDER});

            foreach (var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var assemblyData = GetAssemblyDataByPath(path);

                if (!UpdateReferences(assemblyData)) continue;

                SaveAssemblyDefinition(path, assemblyData);
                AssetDatabase.ImportAsset(path);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static object GetAssemblyDataByPath(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);
            var assemblyData = FROM_JSON_METHOD.Invoke(null, new object[] {asset.text});
            return assemblyData;
        }

        private static bool UpdateReferences(object assemblyData)
        {
            var name = (string) NAME_FIELD.GetValue(assemblyData);

            var references = (string[]) REFS_FIELD.GetValue(assemblyData);
            if (references == null) return false;

            var hasMissingRefs = false;
            var validReferences = new List<string>(references.Length);

            foreach (var reference in references)
            {
                if (!reference.StartsWith("GUID:")) continue;

                var guid = reference.Substring("GUID:".Length);
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(path))
                {
                    Debug.Log($"<color=red>{reference}</color>");
                    hasMissingRefs = true;
                }
                else
                {
                    Debug.Log($"<color=green>{reference}</color>");
                    validReferences.Add(reference);
                }
            }

            REFS_FIELD.SetValue(assemblyData, validReferences.ToArray());

            return hasMissingRefs;
        }

        private static void SaveAssemblyDefinition(string path, object assemblyData)
        {
            var json = (string) TO_JSON_METHOD.Invoke(null, new []{assemblyData});
            File.WriteAllText(path, json);
        }
    }
}
