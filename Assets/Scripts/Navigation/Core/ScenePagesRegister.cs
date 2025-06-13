using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Navigation.Core
{
    internal sealed class ScenePagesRegister: MonoBehaviour
    {
        [SerializeField] private Page[] _scenePages;

        public Page[] Pages => _scenePages;
       
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Frever/Scene Pages List", false, 0)]
        public static void CreateScenePages()
        {
            var gameObj = new GameObject("PagesRegister");
            var scenePages = gameObj.AddComponent<ScenePagesRegister>();
            scenePages.RefreshPagesList();
        }
        
        [Button("Collect Pages")]
        private void RefreshPagesList()
        {
            if (Application.isPlaying) return;
            
            var pages = new List<Page>();
            var roots = gameObject.scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var childrenPages = root.gameObject.GetComponentsInChildren<Page>(true);
                pages.AddRange(childrenPages.Where(item => item != null));
            }

            _scenePages = pages.ToArray();
            transform.SetAsFirstSibling();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gameObject.scene);
        }
        #endif
    }
}