using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class UserPhotoLoader : TextureAssetLoader<PhotoFullInfo, PhotoLoadArgs>
    {
        public UserPhotoLoader(IBridge bridge) : base(bridge)
        {
        }

        protected override IAsset CreateView(Texture2D texture)
        {
            var view = new PhotoAsset();
            view.Init(Model, texture);
            return view;
        }
    }
}