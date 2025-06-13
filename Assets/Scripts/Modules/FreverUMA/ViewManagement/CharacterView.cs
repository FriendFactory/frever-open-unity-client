using System;
using Bridge.Models.ClientServer.Assets;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.FreverUMA.ViewManagement
{
    public sealed class CharacterView
    {
        public readonly long CharacterId;
        public readonly DynamicCharacterAvatar Avatar;
        public readonly GameObject GameObject;
        public readonly ViewType ViewType;

        public OutfitFullInfo Outfit { get; private set; }
        public long? OutfitId => Outfit?.Id;
        public float HeelsHeight { get; private set; }
        public float Height { get; private set; }
        public float Width { get; private set; }

        public bool IsEditable => ViewType == ViewType.GeneratedRuntime && Avatar.GetComponent<UMAData>().Validate(false);
        public bool IsDestroyed { get; private set; }

        public event Action Updated;
        public event Action<CharacterView> Released;
        public event Action<CharacterView> Destroyed;

        internal CharacterView(long characterId, OutfitFullInfo outfit, SpawnedViewData viewData)
        {
            CharacterId = characterId;
            Outfit = outfit;
            GameObject = viewData.GameObject;
            Avatar = viewData.Avatar;
            HeelsHeight = viewData.HeelsHeight;
            Height = viewData.CharacterHeight;
            Width = viewData.CharacterWidth;
            ViewType = viewData.ViewType;
        }

        public void ChangeData(OutfitFullInfo outfit, float heelsHeight, float height, float width)
        {
            Outfit = outfit;
            HeelsHeight = heelsHeight;
            Height = height;
            Width = width;
            Updated?.Invoke();
        }
        
        public void ReplaceOutfit(OutfitFullInfo outfit)
        {
            Outfit = outfit;
            Updated?.Invoke();
        }

        public void Release()
        {
            SetActive(false);
            GameObject.transform.SetParent(null);
            Released?.Invoke(this);
        }

        public void SetActive(bool isActive)
        {
            GameObject.SetActive(isActive);
        }
        
        internal void Destroy()
        {
            Object.Destroy(GameObject);
            IsDestroyed = true;
            Destroyed?.Invoke(this);
        }
    }
}