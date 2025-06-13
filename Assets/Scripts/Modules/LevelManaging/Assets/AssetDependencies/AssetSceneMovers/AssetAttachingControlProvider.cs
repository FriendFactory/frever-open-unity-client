using System.Linq;
using Extensions;
using UnityEngine.SceneManagement;

namespace Modules.LevelManaging.Assets.AssetDependencies.AssetSceneMovers
{
    internal sealed class AssetAttachingControlProvider
    {
        private readonly AssetAttachingControl[] _assetAttachingControls;

        public AssetAttachingControlProvider(Scene scene, ISceneObjectHelper sceneObjectHelper)
        {
            _assetAttachingControls = new AssetAttachingControl[]
            {
                new CharacterAttachingControl(scene, sceneObjectHelper), 
                new DefaultAssetAttachingControl(scene, sceneObjectHelper, DbModelType.Vfx),
                new CaptionAttachingControl(scene, sceneObjectHelper)
            };
        }
        
        public AssetAttachingControl GetControl(DbModelType type)
        {
            return _assetAttachingControls.First(x=>x.Type == type);
        }
    }
}