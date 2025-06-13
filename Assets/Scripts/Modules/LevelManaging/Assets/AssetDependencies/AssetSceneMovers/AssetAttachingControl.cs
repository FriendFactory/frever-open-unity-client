using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.LevelManaging.Assets.AssetDependencies.AssetSceneMovers
{
    /// <summary>
    /// Removes asset from scene and moves it to target scene and sets up correct position.
    /// </summary> 
    internal abstract class AssetAttachingControl
    {
        private readonly ISceneObjectHelper _sceneObjectHelper;
        
        public abstract DbModelType Type {get;}
        private readonly Scene _scene;
        
        protected AssetAttachingControl(Scene scene, ISceneObjectHelper sceneObjectHelper)
        {
            _sceneObjectHelper = sceneObjectHelper;
            _scene = scene;
        }

        public virtual void Detach(ISceneObject asset)
        {
            _sceneObjectHelper.MoveAssetToPersistantScene(asset);
        }

        public virtual void Attach(ISceneObject asset, CharacterSpawnPositionInfo spawnPosition, Transform targetParent)
        {
            MoveObjectToScene(asset);
            SetParent(asset, targetParent);
            ResetPosition(asset, spawnPosition);
        }

        public virtual void ResetPosition(ISceneObject asset, CharacterSpawnPositionInfo spawnPosition)
        {
            var transform = asset.GameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        
        private void MoveObjectToScene(ISceneObject gameObject)
        {
            _sceneObjectHelper.MoveAssetToScene(gameObject, _scene);
        }

        protected virtual void SetParent(ISceneObject asset, Transform targetParent)
        {
            asset.GameObject.transform.SetParent(targetParent);
        }
    }

    internal sealed class DefaultAssetAttachingControl : AssetAttachingControl
    {
        public override DbModelType Type { get; }
        
        public DefaultAssetAttachingControl(Scene scene, ISceneObjectHelper sceneObjectHelper, DbModelType type) : base(scene, sceneObjectHelper)
        {
            Type = type;
        }
    }

    internal sealed class CaptionAttachingControl : AssetAttachingControl
    {
        public override DbModelType Type => DbModelType.Caption;
        
        public CaptionAttachingControl(Scene scene, ISceneObjectHelper sceneObjectHelper) : base(scene, sceneObjectHelper)
        {
        }

        public override void Detach(ISceneObject asset)
        {
            base.Detach(asset);
            ((ICaptionAsset)asset).SetCamera(null);
        }
    }

    internal sealed class CharacterAttachingControl : AssetAttachingControl
    {
        public CharacterAttachingControl(Scene scene, ISceneObjectHelper sceneObjectHelper) : base(scene, sceneObjectHelper)
        {
        }

        public override DbModelType Type => DbModelType.Character;

        public override void Attach(ISceneObject asset, CharacterSpawnPositionInfo spawnPosition, Transform targetParent)
        {
            base.Attach(asset, spawnPosition, targetParent);
            var adjustment = GetAdjustment(asset, spawnPosition);
            var characterAsset = (ICharacterAsset)asset;
            characterAsset.SetScale(adjustment?.Scale ?? 1);
        }

        public override void ResetPosition(ISceneObject asset, CharacterSpawnPositionInfo spawnPosition)
        {
            base.ResetPosition(asset, spawnPosition);
            var adjustment = GetAdjustment(asset, spawnPosition);
            if (adjustment == null) return;
            asset.GameObject.transform.localPosition += new Vector3(adjustment.AdjustX, adjustment.AdjustY, adjustment.AdjustZ);
        }

        private static SpawnPositionAdjustment GetAdjustment(ISceneObject asset, CharacterSpawnPositionInfo spawnPosition)
        {
            if (spawnPosition.Adjustments.IsNullOrEmpty()) return null;
            var characterAsset = (ICharacterAsset)asset;
            var adjustment = spawnPosition.Adjustments.FirstOrDefault(x => x.GenderIds.Contains(characterAsset.GenderId));
            return adjustment;
        }
    }
}
