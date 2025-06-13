using System;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.PhotoBooth.Profile;
using Modules.ProfilePhotoEditing;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    internal sealed class ProfilePhotoEditorPresenter: LevelEditorUiBase
    {
        [SerializeField] private ProfilePhotoMaskOverlay _maskOverlay;
        [SerializeField] private ProfilePhotoEditorAssetSelectionManager _assetSelectionViewManager;
        [SerializeField] private UploadingPanel _uploadingPanel;
        [Header("Buttons")]
        [SerializeField] private CanvasGroup _buttonsGroup;
        [SerializeField] private Button _setLocationsButton;
        [SerializeField] private Button _bodyAnimationsButton;

        [Inject] private ILevelManager _levelManager;
        [Inject] private IProfilePhotoEditorDefaults _editorDefaults;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private BaseEditorPageModel _pageModel;
        
        private BodyAnimationAssetSelectionHolder _bodyAnimationsHolder;
        private SetLocationAssetSelectionHolder _setLocationsHolder;
        
        //---------------------------------------------------------------------
        // Messages 
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            _assetSelectionViewManager.Opened += OnAssetSelectionOpened;
            _assetSelectionViewManager.Closed += OnAssetSelectionClosed;
            _levelManager.SetLocationChangeFinished += _uploadingPanel.OnSetLocationChanged;
            
            _setLocationsButton.onClick.AddListener(OnSetLocationsButtonClicked);
            _bodyAnimationsButton.onClick.AddListener(OnBodyAnimationsButtonClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _assetSelectionViewManager.Opened -= OnAssetSelectionOpened;
            _assetSelectionViewManager.Closed -= OnAssetSelectionClosed;
            _levelManager.SetLocationChangeFinished -= _uploadingPanel.OnSetLocationChanged;
            
            _setLocationsButton.onClick.RemoveListener(OnSetLocationsButtonClicked);
            _bodyAnimationsButton.onClick.RemoveListener(OnBodyAnimationsButtonClicked);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(ProfilePhotoType photoType)
        {
            SetupBodyAnimationsView(OnBodyAnimationSelected, _editorDefaults.BodyAnimationCategory, true);
            SetupSpawnPointsView(OnSpawnPointSelected);
            SetupSetLocations(OnSetLocationSelected, showMyAssetsCategory:false);
            
            _bodyAnimationsHolder = new BodyAnimationAssetSelectionHolder(
                new MainAssetSelectorModel[] { BodyAnimationsAssetSelector });
            _setLocationsHolder = new SetLocationAssetSelectionHolder(
                new MainAssetSelectorModel[] { SetLocationAssetSelector });
            
            SetupSelectedItems(LevelManager.TargetEvent);
            SetupSelectorScrollPositions();
            
            _maskOverlay.Initialize(photoType);
            _maskOverlay.Show();

            _uploadingPanel.RefreshState(false);
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------
        
        protected override BaseEditorPageModel PageModel => _pageModel;
        
        protected override ICategory[] GetBodyAnimationCategories()
        {
            return new ICategory[] { GetBodyAnimationCategory(_editorDefaults.BodyAnimationCategory)} ;
        }

        private new void SetupSelectedItems(Event targetEvent, bool silent = true)
        {
            base.SetupSelectedItems(targetEvent, silent);
        }
        
        private void OnAssetSelectionClosed()
        {
            _buttonsGroup.gameObject.SetActive(true);
            _maskOverlay.Show();
        }

        private void OnAssetSelectionOpened()
        {
            _buttonsGroup.gameObject.SetActive(false);
            _maskOverlay.Hide();
        }
        
        private void SetupSelectorScrollPositions()
        {
            _assetSelectionViewManager.SetSelectedItemScrollPositions(BodyAnimationsAssetSelector);
            _assetSelectionViewManager.SetSelectedItemScrollPositions(SetLocationAssetSelector);
        }

        private void ShowAssetSelectionManagerView(AssetSelectorsHolder assetSelectorsHolder)
        {
            _assetSelectionViewManager.gameObject.SetActive(true);
            _assetSelectionViewManager.Initialize(assetSelectorsHolder);
        }

        private void OnSetLocationsButtonClicked()
        {
            ShowAssetSelectionManagerView(_setLocationsHolder);
            _maskOverlay.Hide();
            _buttonsGroup.gameObject.SetActive(false);
        }
        
        private void OnBodyAnimationsButtonClicked()
        {
            ShowAssetSelectionManagerView(_bodyAnimationsHolder);
            _maskOverlay.Hide();
            _buttonsGroup.gameObject.SetActive(false);
        }

        private void OnSetLocationSelected(AssetSelectionItemModel setLocation)
        {
            if (!setLocation.IsSelected) return;
            
            HideAssetLoadingSnackBarIfOpened();
            ShowAssetLoadingSnackBar();
            
            var location = (SetLocationFullInfo) setLocation.RepresentedObject;
            var selectedSpawnPositionModel = SpawnPositionAssetSelector.AssetSelectionHandler.SelectedModels.FirstOrDefault();
            
            LevelManager.ChangeSetLocation(location, OnCompleted, spawnPositionId: selectedSpawnPositionModel?.ItemId,
                                           allowChangingAnimations: false);
                
            void OnCompleted(IAsset asset, DbModelType[] otherChangedAssetTypes)
            {
                HideAssetLoadingSnackBarIfOpened();
                
                SpawnPositionAssetSelector.SetCurrentSetLocationId(setLocation.ItemId);
                setLocation.ApplyModel();

                var spawnPosition = LevelManager.TargetEvent.GetTargetSpawnPosition();
                SpawnPositionAssetSelector.SetSelectedItems(new[] { spawnPosition.Id }, silent:true);
            }
        }

        private void OnSpawnPointSelected(AssetSelectionItemModel characterSpawnPosition)
        {
            if (!characterSpawnPosition.IsSelected) return;
            
            var setLocation = LevelManager.TargetEvent.GetSetLocation();
            var parentAsset = SetLocationAssetSelector.Models.FirstOrDefault(item => item.ItemId == characterSpawnPosition.ParentAssetId);
            if (!SetLocationAssetSelector.AssetSelectionHandler.IsItemAlreadySelected(parentAsset.ItemId, parentAsset.CategoryId))
            {
                parentAsset.SetIsSelected(true);
                return;
            }

            var isParentAssetLoaded = setLocation.Id == parentAsset.ItemId;
            if (!isParentAssetLoaded) return;

            var spawnPosition =  setLocation.GetSpawnPositions()
                .First(spawnPos => spawnPos.Id == characterSpawnPosition.ItemId);

            LevelManager.ChangeCharacterSpawnPosition(spawnPosition, false);
        }

        private void OnBodyAnimationSelected(AssetSelectionItemModel bodyAnimation)
        {
            if (!bodyAnimation.IsSelected) return;
            
            LevelManager.ChangeBodyAnimation(bodyAnimation.RepresentedObject as BodyAnimationInfo, null);
        }

        private void ShowAssetLoadingSnackBar()
        {
            _snackBarHelper.ShowAssetLoadingSnackBar(Single.PositiveInfinity);
        }

        private void HideAssetLoadingSnackBarIfOpened()
        {
            _snackBarHelper.HideSnackBar(SnackBarType.AssetLoading);
        }
    }
}