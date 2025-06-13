using System;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Modules.LevelManaging.Assets
{
    public interface ICameraFilterVariantAsset: IAsset<CameraFilterVariantInfo>, IAttachableAsset
    {
        AssetBundle Bundle { get; }
        void SetIntensity(float value);
    }

    internal sealed class CameraFilterVariantAsset : RepresentationAsset<CameraFilterVariantInfo>, ICameraFilterVariantAsset
    {
        private ColorLookup _filterColorLookup;
        
        public AssetBundle Bundle { get; private set; }
        public GameObject GameObject { get; private set; }
        public override DbModelType AssetType => DbModelType.CameraFilterVariant;
        public event Action<ISceneObject,Scene> MovedToScene;
        
        protected override void SetModelActive(bool value)
        {
            if (GameObject == null) return;
            GameObject.SetActive(value);
        }

        public void Init(CameraFilterVariantInfo filterVariantData, GameObject gameObject, AssetBundle bundle)
        {
            BasicInit(filterVariantData);
            Bundle = bundle;
            GameObject = gameObject;
            var volume = GameObject.GetComponent<Volume>();
            _filterColorLookup = volume.sharedProfile.components.Find(component => component is ColorLookup) as ColorLookup;
            GameObject.AddListenerToOnGameObjectMovedToAnotherScene(OnMovedToAnotherScene);
        }

        public void SetIntensity(float value)
        {
            if (_filterColorLookup == null) return;
            _filterColorLookup.contribution.value = value;
        }
        
        private void OnMovedToAnotherScene(Scene scene)
        {
            MovedToScene?.Invoke(this, scene);
        }
    }
}
