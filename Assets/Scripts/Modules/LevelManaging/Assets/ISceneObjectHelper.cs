using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.LevelManaging.Assets
{
    /// <summary>
    ///     Prevents assets from being unloaded accidentally with scene(set location) unloading
    /// </summary>
    public interface ISceneObjectHelper
    {
        void MoveAssetToPersistantScene(ISceneObject asset);
        void MoveAssetToScene(ISceneObject asset, Scene destinationScene);
    }

    internal sealed class SceneObjectHelper: ISceneObjectHelper
    {
        private readonly int _persistantSceneIndex;
        private Scene PersistantScene => SceneManager.GetSceneByBuildIndex(_persistantSceneIndex);
        
        private Transform _holder;

        private Transform Holder
        {
            get
            {
                if (!(_holder is null)) return _holder;
                
                _holder = new GameObject("Assets Holder").transform;
                MoveGameObjectToScene(_holder.gameObject, PersistantScene);
                
                return _holder;
            }
        }
        
        public SceneObjectHelper(int persistantSceneIndex)
        {
            _persistantSceneIndex = persistantSceneIndex;
        }

        public void MoveAssetToPersistantScene(ISceneObject asset)
        {
            MoveObjectToScene(asset, PersistantScene);
            SetParent(asset, Holder);
        }

        public void MoveAssetToScene(ISceneObject asset, Scene destinationScene)
        {
            MoveObjectToScene(asset, destinationScene);
        }

        private void MoveObjectToScene(ISceneObject asset, Scene destinationScene)
        {
            SetParent(asset, null);
            MoveGameObjectToScene(asset.GameObject, destinationScene);
        }

        private void MoveGameObjectToScene(GameObject target, Scene destinationScene)
        {
            SceneManager.MoveGameObjectToScene(target, destinationScene);
        }
        
        private void SetParent(ISceneObject target, Transform parent)
        {
            target.GameObject.SetParent(parent);
        }
    }
}