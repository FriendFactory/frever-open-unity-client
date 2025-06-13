using Bridge.Models.ClientServer.Assets;
using Extensions;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    public interface IPhotoAsset: ITextureAsset<PhotoFullInfo>
    {
    }

    internal sealed class PhotoAsset : TextureBaseAsset<PhotoFullInfo>, IPhotoAsset
    {
        public override DbModelType AssetType => DbModelType.UserPhoto;

        public void Init(PhotoFullInfo representation, Texture2D photo)
        {
            BasicInit(representation);
            Texture = photo;
        }

        protected override void SetModelActive(bool value) { }
    }
}
