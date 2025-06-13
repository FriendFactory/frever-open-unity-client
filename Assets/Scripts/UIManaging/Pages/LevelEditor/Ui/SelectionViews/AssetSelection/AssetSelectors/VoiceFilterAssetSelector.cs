using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;
using UnityEngine;
using Event = Models.Event;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    internal sealed class VoiceFilterAssetSelector : MainAssetSelectorModel
    {
        private const string SAMPLE_CLIP_PATH = "AudioMixers/TestingVoice";
        
        private readonly AudioClip _sampleClip;
        private readonly AudioSourceManager _audioSourceManager;
        private readonly MetadataStartPack _metadata;
        private readonly Func<bool> _allowNoVoiceEffectDelegate;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override DbModelType AssetType => DbModelType.VoiceFilter;
        public bool PlaySampleOnSelected { get; set; } = true;

        private bool AllowNoVoiceEffect => _allowNoVoiceEffectDelegate();
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public VoiceFilterAssetSelector(string displayName, ICategory[] categories, MetadataStartPack metadata, 
            AudioSourceManager audioSourceManager, Func<bool> allowNoVoiceEffectDelegate) 
            : base(displayName, categories)
        {
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(1, false);
            SetAssetSelectionHandler(assetSelectionHandler);
            _sampleClip =  Resources.Load<AudioClip>(SAMPLE_CLIP_PATH);
            _metadata = metadata;
            _audioSourceManager = audioSourceManager;
            _allowNoVoiceEffectDelegate = allowNoVoiceEffectDelegate;
            OnSelectedItemChangedByUserEvent += PlaySample;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override Task DownloadInitialPage(long categoryId, bool force = false, CancellationToken token = default)
        {
            SetupVoiceFilters();
            return Task.CompletedTask;
        }

        public override void SetSelectedItemsAsInEvent(ILevelManager levelManager, Event @event,
                IDataFetcher dataFetcher = null,
                bool silent = true)
        {
            SetupVoiceFilters();
            
            base.SetSelectedItemsAsInEvent(levelManager, @event, dataFetcher, silent);

            if (!silent)
            {
                AssetSelectionHandler.UnselectAllSelectedItems();
            }

            var targetCharacterController = @event.TargetCharacterSequenceNumber > -1 
                ? @event.GetTargetCharacterController()
                : @event.GetFirstCharacterController();
        
            var voiceFilterController = targetCharacterController.CharacterControllerFaceVoice.FirstOrDefault();
            
            if (voiceFilterController?.VoiceFilterId == null)
            {
                SetSelectedItems(silent: silent);
                return;
            }
            
            SetSelectedItems(new[] { (long) voiceFilterController.VoiceFilterId }, silent: silent);
        }
        
        public override bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            var targetItem = Models.FirstOrDefault(model => model.ItemId == @event.GetVoiceFilter().Id);
            
            return targetItem != null && targetItem.HideOnInitialize 
                || AssetSelectionHandler.SelectedModels.Count > 0 && AssetSelectionHandler.SelectedModels[0].ItemId == @event.GetVoiceFilter().Id;
        }

        private void PlaySample(AssetSelectionItemModel model)
        {
            if (PlaySampleOnSelected)
            {
                _audioSourceManager.CharacterAudioSource.PlayOneShot(_sampleClip);
            }
        }
        
        private void SetupVoiceFilters()
        {
            var alreadySetup = AllItems.Count > 0;
            if (alreadySetup)
            {
                RefreshHidOnInitialization();
                return;
            }
            
            var voiceFilters = _metadata.VoiceFilters.OrderBy(x => x.SortOrder).ToArray();
            var index = 0;
            var items = voiceFilters.Select(filter => new AssetSelectionVoiceFilterModel(
                                                index++,
                                                Resolution._128x128,
                                                filter,
                                                GetCategoryName(filter)))
                                    .ToArray();

            AddItems(items, true);
            
            RefreshHidOnInitialization();
        }

        private void RefreshHidOnInitialization()
        {
            var noVoiceEffect = Models.FirstOrDefault();
            
            if (noVoiceEffect != null)
            {
                noVoiceEffect.HideOnInitialize = !AllowNoVoiceEffect;
            }
        }

        private string GetCategoryName(VoiceFilterFullInfo voiceFilter)
        {
            return _metadata.VoiceFilterCategories.First(x => x.Id == voiceFilter.VoiceFilterCategoryId).Name;
        }
    }
}