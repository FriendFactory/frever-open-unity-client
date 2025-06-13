using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.LevelManaging.Editing
{
    //Provides list with already loaded assets to prevent their unloading 
    [UsedImplicitly]
    internal sealed class ReusedAssetsAlgorithm
    {
        private readonly IAssetManager _assetManager;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public ReusedAssetsAlgorithm(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        public IAsset[] GetAlreadyLoadedAssetsUsedBy(params Event[] events)
        {
            var reusedAssets = new List<IAsset>();
            foreach (var ev in events)
            {
                reusedAssets.AddRange(GetLoadedAssetForEvent(ev));
            }

            return reusedAssets.Distinct().ToArray();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private IEnumerable<IAsset> GetLoadedAssetForEvent(Event currentEvent)
        {
            var allLoadedAssets = _assetManager.GetAllLoadedAssets().ToArray();
            var loadedAssetsForEvent = new List<IAsset>();

            SelectCharacterRelatedAssets(currentEvent, allLoadedAssets, ref loadedAssetsForEvent);
            SelectVfx(currentEvent, allLoadedAssets, ref loadedAssetsForEvent);
            SelectCameraAnimation(currentEvent, allLoadedAssets, ref loadedAssetsForEvent);
            SelectSetLocation(currentEvent, allLoadedAssets, loadedAssetsForEvent);
            SelectSetLocationMedia(currentEvent, allLoadedAssets, loadedAssetsForEvent);
            SelectCameraFilter(currentEvent, allLoadedAssets, loadedAssetsForEvent);
            SelectMusic(currentEvent, allLoadedAssets, ref loadedAssetsForEvent);
            SelectCaptions(currentEvent, allLoadedAssets, loadedAssetsForEvent);

            return loadedAssetsForEvent;
        }

        private static void SelectCharacterRelatedAssets(Event currentEvent, IAsset[] allLoadedAssets, ref List<IAsset> loadedAssetsForEvent)
        {
            foreach (var charController in currentEvent.CharacterController)
            {
                SelectCharacter(charController, allLoadedAssets, loadedAssetsForEvent);
                SelectVoiceFilter(charController, allLoadedAssets, loadedAssetsForEvent);
                SelectFaceAnimation(charController, allLoadedAssets, loadedAssetsForEvent);
                SelectBodyAnimation(charController, allLoadedAssets, ref loadedAssetsForEvent);
            }
        }

        private void SelectCaptions(Event currentEvent, IAsset[] allLoadedAssets, List<IAsset> loadedAssetsForEvent)
        {
            foreach (var caption in currentEvent.Caption)
            {
                CollectReusedAsset(caption, allLoadedAssets, loadedAssetsForEvent);
            }
        }

        private static void SelectMusic(Event currentEvent, IAsset[] allLoadedAssets, ref List<IAsset> loadedAssetsForEvent)
        {
            var music = currentEvent.GetMusic();
            if (music == null) return;
            CollectReusedAsset(music.GetModelType(), music.Id, allLoadedAssets, ref loadedAssetsForEvent);
        }

        private static void SelectCameraFilter(Event currentEvent, IAsset[] allLoadedAssets, List<IAsset> loadedAssetsForEvent)
        {
            var cameraFilterVariant = currentEvent.GetCameraFilterVariant();
            if (cameraFilterVariant == null) return;
            CollectReusedAsset(cameraFilterVariant.GetModelType(), cameraFilterVariant.Id, allLoadedAssets, ref loadedAssetsForEvent);
        }

        private static void SelectSetLocation(Event currentEvent, IAsset[] allLoadedAssets, List<IAsset> loadedAssetsForEvent)
        {
            var setLocation = currentEvent.GetSetLocation();
            if (setLocation == null) return;
            var type = setLocation.GetModelType();
            CollectReusedAsset(type, setLocation.Id, allLoadedAssets, ref loadedAssetsForEvent);
        }
        
        private static void SelectSetLocationMedia(Event currentEvent, IAsset[] allLoadedAssets, List<IAsset> loadedAssetsForEvent)
        {
            var controller = currentEvent.GetSetLocationController();
            if (controller.VideoClipId.HasValue)
            {
                CollectReusedAsset(DbModelType.VideoClip, controller.VideoClipId.Value, allLoadedAssets, ref loadedAssetsForEvent);
            }

            if (controller.PhotoId.HasValue)
            {
                CollectReusedAsset(DbModelType.UserPhoto, controller.PhotoId.Value, allLoadedAssets, ref loadedAssetsForEvent);
            }
            
            if (controller.SetLocationBackgroundId.HasValue)
            {
                CollectReusedAsset(DbModelType.SetLocationBackground, controller.SetLocationBackgroundId.Value, allLoadedAssets, ref loadedAssetsForEvent);
            }
        }

        private static void SelectCameraAnimation(Event currentEvent, IAsset[] allLoadedAssets, ref List<IAsset> loadedAssetsForEvent)
        {
            var cameraAnimation = currentEvent.GetCameraAnimation();
            if (cameraAnimation == null) return;
            var type = cameraAnimation.GetModelType();
            CollectReusedAsset(type, cameraAnimation.Id, allLoadedAssets, ref loadedAssetsForEvent);
        }

        private static void SelectVfx(Event currentEvent, IAsset[] allLoadedAssets, ref List<IAsset> loadedAssetsForEvent)
        {
            var vfx = currentEvent.GetVfx();
            if (vfx == null) return;
            var type = vfx.GetModelType();
            CollectReusedAsset(type, vfx.Id, allLoadedAssets, ref loadedAssetsForEvent);
        }

        private static void SelectBodyAnimation(CharacterController charController, IAsset[] allLoadedAssets, ref List<IAsset> loadedAssetsForEvent)
        {
            var bodyAnimation = charController.GetBodyAnimation();
            if (bodyAnimation == null) return;

            var type = bodyAnimation.GetModelType();
            CollectReusedAsset(type, bodyAnimation.Id, allLoadedAssets, ref loadedAssetsForEvent);
        }

        private static void SelectFaceAnimation(CharacterController charController, IAsset[] allLoadedAssets, List<IAsset> loadedAssetsForEvent)
        {
            var faceAnimation = charController.GetFaceAnimation();
            if (faceAnimation == null) return;
            var type = faceAnimation.GetModelType();
            CollectReusedAsset(type, faceAnimation.Id, allLoadedAssets, ref loadedAssetsForEvent);
        }

        private static void SelectVoiceFilter(CharacterController charController, IAsset[] allLoadedAssets, List<IAsset> loadedAssetsForEvent)
        {
            var voiceFilter = charController.GetVoiceFilter();
            if (voiceFilter == null) return;
            var type = voiceFilter.GetModelType();
            CollectReusedAsset(type, voiceFilter.Id, allLoadedAssets, ref loadedAssetsForEvent);
        }

        private static void SelectCharacter(CharacterController charController, IAsset[] allLoadedAssets, List<IAsset> loadedAssetsForEvent)
        {
            var character = charController.Character;
            var outfitId = charController.OutfitId;
            var characterAsset = allLoadedAssets.Where(x => x.AssetType == DbModelType.Character).Cast<ICharacterAsset>()
                           .FirstOrDefault(x => x.Id == character.Id && x.OutfitId.Compare(outfitId));
            if (characterAsset == null) return;
            loadedAssetsForEvent.Add(characterAsset);
        }

        private static void CollectReusedAsset(DbModelType type, long id, IEnumerable<IAsset> allAssets, ref List<IAsset> targetEventAssets)
        {
            var asset = allAssets.FirstOrDefault(x => x.AssetType == type && x.Id == id);
            if (asset == null) return;
            targetEventAssets.Add(asset);
        }

        private static void CollectReusedAsset<T>(T model, IEnumerable<IAsset> allAssets, List<IAsset> targetEventAssets) where T : IEntity
        {
            var asset = allAssets.FirstOrDefault(x => x is RepresentationAsset<T> representationAsset && representationAsset.RepresentedModel.Compare(model));
            if (asset == null) return;
            targetEventAssets.Add(asset);
        }
    }
}