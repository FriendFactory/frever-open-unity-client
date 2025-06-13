using Bridge.Models.Common;
using Modules.LevelManaging.Assets;
using SharedAssetBundleScripts.Runtime.SetLocationScripts;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal abstract class TextureAssetPlayer<TAsset, TModel>: GenericTimeDependAssetPlayer<TAsset> 
        where TAsset : ITextureAsset<TModel> 
        where TModel: IEntity
    {
        private UserPhotoVideoPlayer[] _targetPlayers;

        public void SetTargetPlayers(params UserPhotoVideoPlayer[] players)
        {
            _targetPlayers = players;
        }
        
        public override void Simulate(float time)
        {
            OnPlay();
        }

        protected override void OnPlay()
        {
            foreach (var player in _targetPlayers)
            {
                if (Target.Texture)
                {
                    player.ShowTexture(Target.Texture);
                }
            }
        }

        protected override void OnPause()
        {
        }

        protected override void OnResume()
        {
        }

        protected override void OnStop()
        {
        }
    }
}