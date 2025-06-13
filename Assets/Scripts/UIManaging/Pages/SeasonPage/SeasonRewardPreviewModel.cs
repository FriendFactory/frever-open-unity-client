using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Gamification.Reward;
using Configs;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.EditorsCommon;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.SeasonPage
{
    [UsedImplicitly]
    public class SeasonRewardPreviewModel
    {
        public bool IsLocked { get; set; }
        public IRewardModel Reward { get; set; }
        public int TargetLevel { get; set; }
        public int CurrentLevel { get; set; }

        [Inject] private readonly IBridge _bridge;
        [Inject] private readonly PageManager _pageManager;
        [Inject] private readonly PageManagerHelper _pageManagerHelper;
        [Inject] private readonly CharacterManager _characterManager;
        [Inject] private readonly IEditorSettingsProvider _editorSettingsProvider;
        [Inject] private readonly IDataFetcher _dataFetcher;
        [Inject] private readonly PopupManagerHelper _popupManagerHelper;
        [Inject] private readonly CharacterManagerConfig _characterManagerConfig;
        
        private CancellationTokenSource _cancellationSource;
        
        public async void DownloadThumbnail(Action<Texture2D> onSuccess)
        {
            CancelLoading();
            _cancellationSource = new CancellationTokenSource();

            var result = await _bridge.GetThumbnailAsync(Reward, Resolution._512x512, true, _cancellationSource.Token);

            if (result != null && result.IsSuccess)
            {
                var texture = result.Object as Texture2D;
                
                if (result.Object is Texture2D)
                {
                    onSuccess?.Invoke(texture);
                }
                else
                {
                    Debug.LogWarning("Wrong thumbnail format.");
                }
            }
            else
            {
                Debug.LogWarning(result?.ErrorMessage);
            }

            _cancellationSource = null;
        }

        public void CancelLoading()
        {
            if (_cancellationSource == null)
            {
                return;
            }
            
            _cancellationSource.Cancel();
            _cancellationSource = null;
        }

        public async void TryAsset(Action onComplete)
        {
            CancelLoading();
            _cancellationSource = new CancellationTokenSource();

            var character = (await _characterManager.GetSelectedCharacterFullInfo()).Clone();
            
            var result = await _bridge.GetRewardWardrobe(Reward, character.GenderId, _cancellationSource.Token);

            if (result.IsError)
            {
                Debug.LogError($"Failed to fetch wardrobe data, reason: {result.ErrorMessage}");
            }

            if (!result.IsSuccess)
            {
                return;
            }

            var wardrobe = result.Model;
            
            character.Wardrobes = new List<WardrobeFullInfo>(character.Wardrobes);

            var slotClipping = _characterManagerConfig.SlotsClippingMatrix.FirstOrDefault(x => x.Slot == 
                    wardrobe.UmaBundle.UmaAssets.FirstOrDefault(asset => asset.SlotId != null && asset.SlotId != 0)
                       ?.SlotName);
            
            if (slotClipping != null)
            {
                character.Wardrobes.RemoveAll(wardrobeToRemove =>
                                                  slotClipping.ClippingSlots.Contains(
                                                      wardrobeToRemove.UmaBundle.UmaAssets
                                                                      .FirstOrDefault(
                                                                           asset => asset.SlotId != null &&
                                                                               asset.SlotId != 0)?.SlotName));
            }
            
            character.Wardrobes.Add(wardrobe);
            
            var args = new UmaEditorArgs
            {
                IsNewCharacter = false,
                BackButtonAction = OnMoveBack,
                ConfirmButtonAction = OnMoveNext,
                Character = character,
                CharacterEditorSettings = (await _editorSettingsProvider.GetDefaultEditorSettings()).CharacterEditorSettings,
                ConfirmActionType = CharacterEditorConfirmActionType.SaveCharacter,
                CategoryTypeId = _dataFetcher.MetadataStartPack.WardrobeCategories.First(category => category.Id == wardrobe.WardrobeCategoryId).WardrobeCategoryTypeId,
                CategoryId = wardrobe.WardrobeCategoryId,
                SubCategoryId = wardrobe.WardrobeSubCategoryIds.Any() ? (long?)wardrobe.WardrobeSubCategoryIds.First() : null,
                OutfitsUsedInLevel = new HashSet<long?>()
            };

            _pageManager.PageDisplayed += OnPageDisplayed;
            _pageManagerHelper.MoveToUmaEditor(args);

            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                
                if (pageData.PageId != args.TargetPage)
                {
                    return;
                }
                
                onComplete?.Invoke();
            }
        }

        public void ExplorePremium()
        {
            _popupManagerHelper.ShowPremiumPassPopup();
        }

        private void OnMoveBack()
        {
            _pageManager.MoveBack();
        }

        private void OnMoveNext()
        {
            _pageManager.MoveBack();
        }

        [UsedImplicitly]
        public class Factory : PlaceholderFactory<SeasonRewardPreviewModel> { }
    }
}