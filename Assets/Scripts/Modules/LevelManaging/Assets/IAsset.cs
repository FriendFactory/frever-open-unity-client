using System;
using Bridge.Models.Common;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.LevelManaging.Assets
{
    public interface ISceneObject
    {
        GameObject GameObject { get; }
        event Action<ISceneObject, Scene> MovedToScene;
    }
    
    public interface IAsset<out T> : IAsset where T:IEntity
    {
        T RepresentedModel { get; }
    }
    
    public interface IAsset
    {
        event Action<long> Destroyed;
        DbModelType AssetType { get; }
        long Id { get; }
        IEntity Entity { get; }
        bool IsActive { get; }
        void SetActive(bool value);
        void CleanUp();
        void PrepareForUnloading();
    }

    public interface IAttachableAsset : IAsset, ISceneObject
    {
    }
}