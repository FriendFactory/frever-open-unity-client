using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using UnityEngine;
using CameraAnimationTemplate = Bridge.Models.ClientServer.StartPack.Metadata.CameraAnimationTemplate;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public abstract class AssetSelectionItemModel
    {
        public event Action<AssetSelectionItemModel> OnIsSelectedChangedByUserEvent;
        public event Action<AssetSelectionItemModel> OnIsSelectedChangedEvent;
        public event Action<AssetSelectionItemModel> OnIsSelectedChangedSilentEvent;
        public event Action<AssetSelectionItemModel> OnModelApplied;

        public IThumbnailOwner ThumbnailOwner { get; private set; }
        public Resolution Resolution { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsPurchased { get; set; }
        public bool HideOnInitialize { get; set; }
        public long ItemId { get; private set; }
        public long CategoryId { get; private set; }
        public long ParentAssetId { get; protected set; } = -1;
        public string CategoryName { get; private set; }
        public string DisplayName { get; private set; }
        public IEntity RepresentedObject { get; private set; }
        public bool IsLocked { get; private set; }
        public bool IsNew { get; protected set; }
        public bool IsDefault { get; set; }
        public IDictionary<string, int> ItemIndices { get; } = new Dictionary<string, int>();
        public bool IsProxy { get; set; }

        protected AssetSelectionItemModel(long itemId, long categoryId, string categoryName, string displayName, IThumbnailOwner thumbnailOwner, Resolution resolution, IEntity representedObject)
        {
            ItemId = itemId;
            CategoryId = categoryId;
            DisplayName = displayName;
            ThumbnailOwner = thumbnailOwner;
            Resolution = resolution;
            RepresentedObject = representedObject;
            CategoryName = categoryName;
        }

        public virtual void UpdateProxy(AssetSelectionItemModel realModel)
        {
            if (!IsProxy)
            {
                Debug.LogError("This asset selection item isn't proxy, yet you're trying to replace it");
                return;
            }
            
            ThumbnailOwner = realModel.ThumbnailOwner;
            Resolution = realModel.Resolution;
            IsPurchased = realModel.IsPurchased;
            HideOnInitialize = realModel.HideOnInitialize;
            ItemId = realModel.ItemId;
            CategoryId = realModel.CategoryId;
            ParentAssetId = realModel.ParentAssetId;
            CategoryName = realModel.CategoryName;
            DisplayName = realModel.DisplayName;
            RepresentedObject = realModel.RepresentedObject;
            IsNew = realModel.IsNew;
            IsDefault = realModel.IsDefault;
            
            foreach (var pair in realModel.ItemIndices)
            {
                ItemIndices[pair.Key] = pair.Value;
            }
            
            IsProxy = false;
        }

        public void TrySetIsSelectedByUser() 
        {
            if (IsLocked)
            {
                return;
            } 
            
            OnIsSelectedChangedByUserEvent?.Invoke(this);
        }
        
        public virtual void SetIsSelected(bool value, bool silent = false)
        {
            var changed = IsSelected != value;
            
            IsSelected = value;
            
            OnIsSelectedChangedSilentEvent?.Invoke(this);

            if (silent)
            {
                OnModelApplied?.Invoke(this);
            }
            else if (changed)
            {
                OnIsSelectedChangedEvent?.Invoke(this);
            }
        }

        public void ApplyModel()
        {
            OnModelApplied?.Invoke(this);
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public void Unlock()
        {
            IsLocked = false;
        }
    }
    
    public class AssetTimeTrackedModel<T> : AssetSelectionItemModel where T : IThumbnailOwner, INewTrackable
    {
        protected AssetTimeTrackedModel(long itemId, long categoryId, string categoryName, string displayName, T model, Resolution resolution, IEntity representedObject) 
            : base(itemId, categoryId, categoryName, displayName, model, resolution, representedObject)
        {
            IsNew = model?.IsNew ?? false;
        }
    }

    public sealed class AssetSelectionCameraModel : AssetSelectionItemModel
    {
        public AssetSelectionCameraModel(int index, Resolution resolution, CameraAnimationTemplate cameraAnimationTemplate, string categoryName, long categoryId) : 
            base(cameraAnimationTemplate.Id, categoryId, categoryName, 
                cameraAnimationTemplate.DisplayName, cameraAnimationTemplate, resolution, cameraAnimationTemplate)
        {
            ItemIndices["CategoryId:" + CategoryId] = index;
        }
        
        public AssetSelectionCameraModel(Resolution resolution, CameraAnimationTemplate cameraAnimationTemplate, string categoryName, long categoryId) : 
            base(cameraAnimationTemplate.Id, categoryId, categoryName, 
                 cameraAnimationTemplate.DisplayName, cameraAnimationTemplate, resolution, cameraAnimationTemplate) { }
        
        public override void SetIsSelected(bool value, bool silent = false)
        {
            base.SetIsSelected(value, silent);

            if (!silent)
            {
                ApplyModel();
            }
        }
    }

    public sealed class AssetSelectionBodyAnimationModel : AssetTimeTrackedModel<BodyAnimationInfo>
    {
        public AssetSelectionBodyAnimationModel(Resolution resolution, BodyAnimationInfo bodyAnimation, string categoryName) : 
            base(bodyAnimation.Id, bodyAnimation.BodyAnimationCategoryId, categoryName, bodyAnimation.Name, bodyAnimation, resolution, bodyAnimation)
        {
        }
        
        public override void SetIsSelected(bool value, bool silent = false)
        {
            base.SetIsSelected(value, silent);

            if (!silent)
            {
                ApplyModel();
            }
        }
    }

    public sealed class AssetSelectionVfxModel : AssetTimeTrackedModel<VfxInfo>
    {
        public AssetSelectionVfxModel(Resolution resolution, VfxInfo vfx, string categoryName) : 
            base(vfx.Id, vfx.VfxCategoryId, categoryName, vfx.Name, vfx, resolution, vfx)
        {
        }
        
        public override void SetIsSelected(bool value, bool silent = false)
        {
            base.SetIsSelected(value, silent);

            if (!silent)
            {
                ApplyModel();
            }
        }
    }

    public sealed class AssetSelectionSetLocationModel : AssetTimeTrackedModel<SetLocationFullInfo>
    {
        public AssetSelectionSetLocationModel(Resolution resolution, SetLocationFullInfo setLocation, string categoryName) : 
            base(setLocation.Id, setLocation.CategoryId, categoryName, setLocation.Name, setLocation, resolution, setLocation)
        {

        }
        
        public override void SetIsSelected(bool value, bool silent = false)
        {
            base.SetIsSelected(value, silent);

            if (!silent && !value)
            {
                ApplyModel();
            }
        }
    }

    public sealed class AssetSelectionCharacterModel : AssetTimeTrackedModel<CharacterInfo>
    {
        public bool HasAccess { get; private set; }
        
        public AssetSelectionCharacterModel(Resolution resolution, CharacterInfo character, long categoryId, bool checkAccess) : 
            base(character.Id, categoryId, string.Empty, character.Name, character, resolution, character)
        {
            HasAccess = !checkAccess || character.Access != CharacterAccess.Private;
        }
        
        public AssetSelectionCharacterModel(CharacterFullInfo character) :
            base(character.Id, 0, string.Empty, character.Name, null, 0, character)
        {
            IsProxy = true;
        }
        
        public override void SetIsSelected(bool value, bool silent = false)
        {
            base.SetIsSelected(value, silent);

            if (!silent)
            {
                ApplyModel();
            }
        }

        public override void UpdateProxy(AssetSelectionItemModel realModel)
        {
            if (realModel is AssetSelectionCharacterModel characterModel && IsProxy)
            {
                HasAccess = characterModel.HasAccess;
            }
            
            base.UpdateProxy(realModel);
        }
    }

    public sealed class AssetSelectionSpawnPositionModel : AssetSelectionItemModel
    {
        public AssetSelectionSpawnPositionModel(int index, Resolution resolution, CharacterSpawnPositionInfo characterSpawnPosition, long categoryId, long setLocationId, string categoryName) : 
            base(characterSpawnPosition.Id, categoryId, categoryName, characterSpawnPosition.Name, characterSpawnPosition, resolution, characterSpawnPosition)
        {
            ItemIndices["SetLocationId:" + setLocationId] = index;
            ParentAssetId = setLocationId;
        }
        
        public override void SetIsSelected(bool value, bool silent = false)
        {
            base.SetIsSelected(value, silent);

            if (!silent)
            {
                ApplyModel();
            }
        }
    }

    public sealed class AssetSelectionVoiceFilterModel : AssetSelectionItemModel
    {
        public AssetSelectionVoiceFilterModel(int index, Resolution resolution, VoiceFilterFullInfo voiceFilter, string categoryName) : 
            base(voiceFilter.Id, voiceFilter.VoiceFilterCategoryId, categoryName, voiceFilter.Name, voiceFilter, resolution, voiceFilter)
        {
            ItemIndices["CategoryId:" + CategoryId] = index;
        }
        
        public override void SetIsSelected(bool value, bool silent = false)
        {
            base.SetIsSelected(value, silent);

            if (!silent)
            {
                ApplyModel();
            }
        }
    }

    public sealed class AssetSelectionOutfitModel : AssetSelectionItemModel
    {
        public AssetSelectionOutfitModel(int index, Resolution resolution, OutfitShortInfo outfit) :
            base(outfit.Id, outfit.SaveMethod == SaveOutfitMethod.Manual ? 0 
                 : outfit.SaveMethod == SaveOutfitMethod.Automatic ? 1 : 2, 
                 string.Empty, outfit.Name, outfit, resolution, outfit)
        {
            ItemIndices["CategoryId:" + CategoryId] = index;
        }
    }

    public sealed class AssetSelectionCameraFilterModel : AssetTimeTrackedModel<CameraFilterInfo>
    {
        public AssetSelectionCameraFilterModel(Resolution resolution, CameraFilterInfo cameraFilter, string categoryName) :
            base(cameraFilter.Id, cameraFilter.CameraFilterCategoryId, categoryName, cameraFilter.Name, cameraFilter, resolution, cameraFilter)
        {
            IsNew = cameraFilter.IsNew;
        }
        
        public override void SetIsSelected(bool value, bool silent = false)
        {
            base.SetIsSelected(value, silent);

            if (!silent)
            {
                ApplyModel();
            }
        }
    }

    public sealed class AssetSelectionCameraFilterVariantModel : AssetTimeTrackedModel<CameraFilterVariantInfo>
    {
        public readonly CameraFilterInfo CameraFilter;
        
        public AssetSelectionCameraFilterVariantModel(int index, Resolution resolution, CameraFilterVariantInfo cameraFilterVariant, long categoryId, CameraFilterInfo cameraFilter, string categoryName) :
            base(cameraFilterVariant.Id, categoryId, categoryName, cameraFilterVariant.Name,  cameraFilterVariant, resolution, cameraFilterVariant)
        {
            ItemIndices["CameraFilterId:" + cameraFilter.Id] = index;
            ParentAssetId = cameraFilter.Id;
            CameraFilter = cameraFilter;
        }
        
        public override void SetIsSelected(bool value, bool silent = false)
        {
            base.SetIsSelected(value, silent);

            if (!silent)
            {
                ApplyModel();
            }
        }
    }
}