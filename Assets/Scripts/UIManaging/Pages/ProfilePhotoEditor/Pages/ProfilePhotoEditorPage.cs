using System;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.InputHandling;
using Modules.ProfilePhotoEditing;
using Modules.RenderingPipelineManagement;
using Navigation.Core;
using Settings;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    [RequireComponent(typeof(ProfilePhotoEditorPresenter))]
    internal sealed class ProfilePhotoEditorPage : GenericPage<ProfilePhotoEditorPageArgs>
    {
        [SerializeField] private PageHeaderView _headerView;
        [SerializeField] private Button _takeShotButton;

        [Inject] private IProfilePhotoEditor _photoEditor;
        [Inject] private IInputManager _inputManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private IRenderingPipelineManager _renderingPipelineManager;
        [Inject] private BaseEditorPageModel _pageModel;
        [Inject] private IMetadataProvider _metadataProvider;
        [Inject] private LocalUserDataHolder _userData;

        private ProfilePhotoEditorPresenter _photoEditorPresenter;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.ProfilePhotoEditor;
        private MetadataStartPack MetadataStartPack => _metadataProvider.MetadataStartPack;

        //---------------------------------------------------------------------
        // Messages 
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _photoEditorPresenter = GetComponent<ProfilePhotoEditorPresenter>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _photoEditor?.Cleanup();
        }

        //---------------------------------------------------------------------
        // Page 
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _headerView.Init(new PageHeaderArgs(string.Empty, new ButtonArgs(string.Empty, OnBackButtonPressed)));
            _takeShotButton.onClick.AddListener(TakeShotAsync);
        }

        protected override void OnDisplayStart(ProfilePhotoEditorPageArgs args)
        {
            _pageModel.Universe = MetadataStartPack.GetUniverseByGenderId(args.Profile.MainCharacter.GenderId);
            UpdateUserAssetsAndBalance();
            
            if (!_photoEditor.IsReady)
            {
                InitializeEditorAsync(args);
            }
            else
            {
                _photoEditor.EnableCamera = true;
            }
            
            _inputManager.Enable(true);
            _renderingPipelineManager.SetHighQualityPipeline();

            base.OnDisplayStart(args);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _inputManager.Enable(false);
            _photoEditor.EnableCamera = false;

            if (AppSettings.UseOptimizedRenderingScale)
            {
                _renderingPipelineManager.SetDefaultPipeline();
            }
            
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void InitializeEditorAsync(ProfilePhotoEditorPageArgs args)
        {
            _popupManagerHelper.ShowLoadingOverlay(Constants.NavigationMessages.DEFAULT_LEVEL_EDITOR_MESSAGE);
            
            await _photoEditor.InitializeAsync(args.PhotoType);
            
            _photoEditorPresenter.Initialize(args.PhotoType);

            _popupManagerHelper.HideLoadingOverlay();
        }

        private async void TakeShotAsync()
        {
            var photo = await _photoEditor.GetPhotoAsync();

            Manager.MoveNext(PageId.ProfilePhotoPostEditor, new ProfilePhotoPostEditorArgs
            {
                Profile = OpenPageArgs.Profile,
                PhotoType = OpenPageArgs.PhotoType,
                OnConfirmBackPageId = OpenPageArgs.OnConfirmBackPageId,
                Photo = photo,
            });
        }

        private void OnBackButtonPressed()
        {
            Manager.MoveBack();
        }
        
        private async void UpdateUserAssetsAndBalance()
        {
            await _userData.UpdatePurchasedAssetsInfo();
            await _userData.UpdateBalance();
        }
    }
}