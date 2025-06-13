using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    public interface ISetLocationBackgroundAsset: ITextureAsset<SetLocationBackground>
    {
    }
    
    [UsedImplicitly]
    internal sealed class SetLocationBackgroundAsset: TextureBaseAsset<SetLocationBackground>, ISetLocationBackgroundAsset
    {
        public override DbModelType AssetType => DbModelType.SetLocationBackground;

        public void Init(SetLocationBackground model, Texture2D texture)
        {
            BasicInit(model);
            Texture = texture;
        }
        
        protected override void SetModelActive(bool value)
        {
        }
    }
}