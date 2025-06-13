using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Bridge.Results;
using JetBrains.Annotations;
using Modules.CharacterManagement;
using Modules.PhotoBooth.Profile;
using Modules.ProfilePhotoEditing;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    internal sealed class ProfilePhotoPostEditorPage : BaseProfilePhotoPreviewablePage<ProfilePhotoPostEditorArgs>
    {
        [Space]
        [SerializeField] private LoadingOverlay _loadingOverlay;
        [SerializeField] private PageHeaderView _headerView;

        [Inject] private ProfileLocalization _localization;
        
        private IBridge _bridge;
        private SnackBarHelper _snackBarHelper;
        private IProfilePhotoEditor _profilePhotoEditor;
        private CharacterManager _characterManager;
        private LocalUserDataHolder _userDataHolder;

        //---------------------------------------------------------------------
        // Properties 
        //---------------------------------------------------------------------

        public override PageId Id => PageId.ProfilePhotoPostEditor;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(IBridge bridge, SnackBarHelper snackBarHelper, IProfilePhotoEditor profilePhotoEditor,
                               CharacterManager characterManager, LocalUserDataHolder userDataHolder)
        {
            _bridge = bridge;
            _snackBarHelper = snackBarHelper;
            _profilePhotoEditor = profilePhotoEditor;
            _characterManager = characterManager;
            _userDataHolder = userDataHolder;
        }
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            base.OnInit(pageManager);
            
            _headerView.Init(new PageHeaderArgs(string.Empty, new ButtonArgs(string.Empty, OnBackButtonPressed)));
        }

        protected override async void OnForwardButtonPressed()
        {
            _loadingOverlay.Show(true);

            var updatedCharacterResult = await UpdatePhotoAsync();
            if (updatedCharacterResult.IsSuccess) await _userDataHolder.DownloadProfile(false);
            
            _loadingOverlay.Hide();
            
            if (updatedCharacterResult.IsError)
            {
                _snackBarHelper.ShowFailSnackBar(_localization.ProfilePhotoUpdateFailedSnackbarMessage);
                return;
            }

            _characterManager.RefreshCache(updatedCharacterResult.Model);
            
            _snackBarHelper.ShowSuccessDarkSnackBar(_localization.ProfilePhotoUpdateSuccessSnackbarMessage);
            
            _profilePhotoEditor.Cleanup();
            
            Manager.MoveBackTo(OpenPageArgs.OnConfirmBackPageId);
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private async Task<Result<CharacterFullInfo>> UpdatePhotoAsync()
        {
            var characterId = OpenPageArgs.Profile.MainCharacter.Id;
            var file = CreateFileInfoFromArgs();

            return await _bridge.UpdateCharacterThumbnails(characterId, file);
        }

        private FileInfo CreateFileInfoFromArgs()
        {
            var photo = OpenPageArgs.Photo;
            var resolution = GetResolution(OpenPageArgs.PhotoType);
            return new FileInfo(photo, FileExtension.Png, resolution);
        }

        private Resolution GetResolution(ProfilePhotoType photoType)
        {
            switch (photoType)
            {
                case ProfilePhotoType.Background:
                    return Resolution._256x256;
                case ProfilePhotoType.Profile:
                default:
                    return Resolution._128x128;
            }
        }
    }
}