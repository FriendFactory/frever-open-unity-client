using Bridge.Models.Common.Files;
using Bridge.Models.Common;
using Bridge;
using Modules.AssetsStoraging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using UnityEngine;
using Zenject;
using FileInfo = Bridge.Models.Common.Files.FileInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;
using System.Threading;
using Extensions;
using JetBrains.Annotations;

namespace Modules.CharacterManagement
{
    [UsedImplicitly]
    public sealed class OutfitsManager
    {
        private readonly IBridge _bridge;
        
        public event Action<IEntity> OutfitAdded;
        public event Action<IEntity> OutfitDeleted;

        private Dictionary<OutfitShortInfo, OutfitFullInfo> _outfits = new Dictionary<OutfitShortInfo, OutfitFullInfo>();

        public OutfitsManager(IBridge bridge)
        {
            _bridge = bridge;
        }

        #region PUBLIC METHODS
        public async Task SaveOutfit(IEnumerable<long> wardrobeIds, IEnumerable<KeyValuePair<long, int>> sharedColors, SaveOutfitMethod saveOutfitMethod,
                                                        KeyValuePair<Resolution, Texture2D>[] photos, Action<OutfitFullInfo> onSuccess, Action<string> onFail)
        {
            var outfitSharedColors = sharedColors.ConvertToOutfitAndUmaSharedColor();

            var outfit = new OutfitSaveModel()
            {
                Name = null,
                WardrobeIds = wardrobeIds.ToList(),
                SaveMethod = saveOutfitMethod,
                UmaSharedColors = outfitSharedColors
            };
            FillThumbnails(outfit, photos);
            await CreateOutfitInternal(outfit, onSuccess, onFail);
        }

        public async void DeleteOutfit(OutfitShortInfo outfit)
        {
            await DeleteOutfitInternal(outfit);
            OnOutfitDeleted(outfit);
        }

        public bool TryGetOutfit(long id, out KeyValuePair<OutfitShortInfo, OutfitFullInfo> outfitKV)
        {
            outfitKV = _outfits.FirstOrDefault(x => x.Key.Id == id);
            return outfitKV.Key != null;

        }
        public OutfitShortInfo ConvertFullToShortInfo(OutfitFullInfo outfitFullInfo)
        {
            return new OutfitShortInfo()
            {
                Id = outfitFullInfo.Id,
                Files = outfitFullInfo.Files,
                Name = outfitFullInfo.Name,
                SaveMethod = outfitFullInfo.SaveMethod
            };
        }

        public async Task<OutfitFullInfo> GetFullOutfit(OutfitShortInfo outfitShortInfo, CancellationToken token = default)
        {
            if (TryGetOutfit(outfitShortInfo.Id, out var outfitKV) && outfitKV.Value != null)
            {
                return outfitKV.Value;
            }

            var fullOutfit = await GetFullInfoInternal(outfitShortInfo, token);
            return fullOutfit;
        }

        public Task<IEnumerable<OutfitShortInfo>> GetOutfitShortInfoList(int take, int skip, SaveOutfitMethod saveOutfitMethod, long genderId, CancellationToken cancellationToken = default)
        {
            return RequestOutfitListInternal(take, skip, saveOutfitMethod, genderId, cancellationToken);
        }

        public async Task SaveAutosavedAsManual(OutfitShortInfo outfit)
        {
            var fullOutfitResult = await _bridge.GetOutfitAsync(outfit.Id, new CancellationToken());
            if (!fullOutfitResult.IsSuccess)
            {
                Debug.LogError(fullOutfitResult.ErrorMessage);
                return;
            }
            var outfitFullInfo = fullOutfitResult.Model;

            var saveModel = new OutfitSaveModel()
            {
                Id = outfit.Id,
                Files = outfitFullInfo.Files,
                Name = outfitFullInfo.Name,
                SaveMethod = SaveOutfitMethod.Manual,
                UmaSharedColors = outfitFullInfo.UmaSharedColors.ToList(),
                WardrobeIds = outfitFullInfo.Wardrobes.Select(x => x.Id).ToList()
            };

            await CreateOutfitInternal(saveModel, null, null);
            outfit.SaveMethod = SaveOutfitMethod.Manual;
        }
        #endregion

        #region CLIENT-SERVER
        private async Task CreateOutfitInternal(OutfitSaveModel outfit, Action<OutfitFullInfo> onSuccess, Action<string> onFail)
        {
            var result = await _bridge.SaveOutfitAsync(outfit);
            onSuccess += OnOutfitSaved;
            if (!result.IsSuccess)
            {
                onFail?.Invoke("One of your assets you are currently wearing have been depublished. Please create a new character or change assets to be able to save.");
                return;
            }
            var outfitData = result.Model;
            onSuccess?.Invoke(outfitData);
        }

        private async Task DeleteOutfitInternal(OutfitShortInfo outfit)
        {
            var result = await _bridge.DeleteOutfit(outfit.Id);
            if (!result.IsSuccess)
            {
                Debug.LogError(result.ErrorMessage);
            }
        }

        private async Task<IEnumerable<OutfitShortInfo>> RequestOutfitListInternal(int take, int skip, SaveOutfitMethod saveOutfitMethod, long genderId, CancellationToken cancellationToken = default)
        {
            var result = await _bridge.GetOutfitListAsync(take, skip, genderId, saveOutfitMethod, cancellationToken);
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return null;
            }
            return result.Models;
        }

        private async Task<OutfitFullInfo> GetFullInfoInternal(OutfitShortInfo outfitShort, CancellationToken token = default)
        {
            if (TryGetOutfit(outfitShort.Id, out var outfitKV) && outfitKV.Value != null)
            {
                return outfitKV.Value;
            }

            var result = await _bridge.GetOutfitAsync(outfitShort.Id, token);
            if (!result.IsSuccess)
            {
                Debug.LogError(result.ErrorMessage);
                return null;
            }
            AddFullInfoToOutfits(result.Model);
            return result.Model;
        }
        #endregion

        #region HELPERS

        private void FillThumbnails(IThumbnailOwner thumbnailOwner, KeyValuePair<Resolution, Texture2D>[] thumbnails)
        {
            thumbnailOwner.Files = new List<FileInfo>();
            foreach (var thumbnail in thumbnails)
            {
                thumbnailOwner.Files.Add(new FileInfo(thumbnail.Value, FileExtension.Png, thumbnail.Key));
            }
        }

        private void AddFullInfoToOutfits(OutfitFullInfo outfitFull)
        {
            if (TryGetOutfit(outfitFull.Id, out var valuePair))
            {
                _outfits[valuePair.Key] = outfitFull;   
            }
        }

        private void OnOutfitSaved(OutfitFullInfo outfit)
        {
            if (TryGetOutfit(outfit.Id, out var valuePair))
            {
                _outfits[valuePair.Key] = outfit;
            }
            else
            {
                _outfits.Add(ConvertFullToShortInfo(outfit), outfit);
            }
            OutfitAdded?.Invoke(outfit);
        }

        private void OnOutfitDeleted(OutfitShortInfo outfit)
        {
            OutfitDeleted?.Invoke(outfit);
        }
        #endregion
    }
}