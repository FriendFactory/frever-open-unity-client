using Bridge.Models.Common;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    public interface ITextureAsset<out TEntity> : IAsset<TEntity> where TEntity:IEntity
    {
        Texture2D Texture { get; }
    }
}