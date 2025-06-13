using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorTools
{
    public class TextMeshMaterialScanner : MonoBehaviour
    {
        [MenuItem("Tools/Friend Factory/Scan TextMesh materials in prefabs")]
        private static void CheckPrefabs()
        {
            var prefabGuids = AssetDatabase.FindAssets( "t:Prefab" );
            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var text = go.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    PrintMaterialName(text, guid);
                }
                
                CheckChildren(go.transform, guid);
            }
        }

        [MenuItem("Tools/Friend Factory/Scan TextMesh materials in scene")]
        private static void CheckScene()
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            foreach (var rootGo in rootObjects)
            {
                var text = rootGo.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    PrintMaterialName(text, scene.name);
                }
                    
                CheckChildren(rootGo.transform, scene.name);
            }
        }

        private static void CheckChildren(Transform parent, string guid)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var childTransform = parent.GetChild(i);
                var text = childTransform.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    PrintMaterialName(text, guid);
                }
                
                CheckChildren(childTransform, guid);
            }
        }

        private static void PrintMaterialName(TextMeshProUGUI text, string guid)
        {
            var material = text.materialForRendering;
            if (material.name.Contains("RundText") && material.name.Contains("Shadow"))
            {
                UnityEngine.Debug.Log($"Material name: {material.name} || Path: {AssetDatabase.GUIDToAssetPath(guid)} || GameObject Name: {text.gameObject.name}");
            }
        }
    }
}
