using System;
using Common.BridgeAdapter;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.CharacterManagement;
using UnityEngine;

namespace Modules.LevelManaging.Editing.LevelSaving
{
    [UsedImplicitly]
    internal sealed class LevelSaver
    {
        private readonly ILevelService _levelService;
        private readonly IDefaultThumbnailService _defaultThumbnailService;
        private readonly IBridge _bridge;
        private readonly CharacterOutfitDefaultDataProvider _outfitDefaultDataProvider;
        
        public LevelSaver(IDefaultThumbnailService defaultThumbnailService, ILevelService levelService, IBridge bridge)
        {
            _defaultThumbnailService = defaultThumbnailService;
            _levelService = levelService;
            _bridge = bridge;
            _outfitDefaultDataProvider = new CharacterOutfitDefaultDataProvider();
        }

        public async void SaveLevel(Level level, Action<Level> onSaved, Action<string> onFail)
        {
            if (!await SaveNotSavedOutfits(level))
            {
                onFail?.Invoke("Unable to save your outfit. Please try again later");
                return;
            }
            
            await level.ResetLocalIdsAsync();
            _defaultThumbnailService.FillMissedEventThumbnailsByDefault(level.Event.ToArray());
            
            var result = await _levelService.SaveLevelAsync(level);

            if (result.IsSuccess)
            {
                onSaved?.Invoke(result.Level);
            }
            else
            {
                onFail?.Invoke(result.ErrorMessage);
            }
        }
        
        private async Task<bool> SaveNotSavedOutfits(Level level)
        {
            var controllersWithNotSavedOutfits = level.Event.SelectMany(x => x.CharacterController)
                                                            .Where(x => x.OutfitId is < 0)
                                                            .GroupBy(x => x.OutfitId.Value);
            foreach (var controllersWithCustomOutfit in controllersWithNotSavedOutfits)
            {
                var outfit = controllersWithCustomOutfit.First().Outfit;
                var outfitSaveModel = new OutfitSaveModel
                {
                    SaveMethod = SaveOutfitMethod.Automatic,
                    WardrobeIds = outfit.Wardrobes.Select(x => x.Id).ToList(),
                    Files = outfit.Files.IsNullOrEmpty() ? _outfitDefaultDataProvider.GetDefaultFiles() : outfit.Files,
                    UmaSharedColors = outfit.UmaSharedColors.ToList()
                };

                var resp = await _bridge.SaveOutfitAsync(outfitSaveModel);
                if (resp.IsError)
                {
                    Debug.LogError($"Failed to save outfit\n{JsonUtility.ToJson(outfitSaveModel, true)}");
                    return false;
                }

                foreach (var controller in controllersWithCustomOutfit)
                {
                    controller.OutfitId = resp.Model.Id;
                    controller.Outfit = resp.Model;
                }
            }

            return true;
        }
    }
}
