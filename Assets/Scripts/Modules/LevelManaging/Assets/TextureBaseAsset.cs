using Bridge.Models.Common;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    internal abstract class TextureBaseAsset<T> : RepresentationAsset<T>, ITextureAsset<T> where T : IEntity
    {
        public Texture2D Texture { get; protected set; }
        
        public override void CleanUp()
        {
            if (!Texture) return;

            Object.Destroy(Texture);
            Texture = null;
        }
    }
}