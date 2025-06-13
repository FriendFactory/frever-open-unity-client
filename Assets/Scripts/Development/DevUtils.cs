using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Development
{
    public static class DevUtils
    {
        public static void ApplyShadersWorkaroundForWinEditor(string sceneName)
        {
            #if UNITY_EDITOR_WIN

            var defaultShader = Shader.Find("Universal Render Pipeline/Lit");
            var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");

            // Replace SetLocation Shaders

            var scene = SceneManager.GetSceneByName(sceneName);
            var rootGameObjects = scene.GetRootGameObjects();

            foreach (var gameObject in rootGameObjects)
            {
                var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                {
                    foreach (var material in renderer.materials)
                    {
                        material.shader = defaultShader;
                    }
                }

                if (gameObject.name == "BackgroundMediaPlayer")
                {
                    var renderer = gameObject.GetComponentInChildren<CanvasRenderer>();

                    for (var i = 0; i < renderer.materialCount; i++)
                    {
                        renderer.GetMaterial(i).shader = unlitShader;
                    }
                }
            }

            // Replace Character Shaders

            var characterAvatars = Object.FindObjectsOfType<DynamicCharacterAvatar>();
            foreach (var gameObject in characterAvatars)
            {
                var skinnedRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var renderer in skinnedRenderers)
                {
                    foreach (var material in renderer.materials)
                    {
                        material.shader = defaultShader;
                    }
                }
            }

            #endif
        }
    }
}