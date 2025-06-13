using System;
using Configs;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.AvatarPreview.Ui
{
    internal sealed class AvatarPreviewPage : GenericPage<AvatarPreviewArgs>
    {
        [SerializeField] private AvatarDisplayView _avatarDisplayView;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _retakeButton;
        [SerializeField] private Button _confirmButton;

        [Inject] private AvatarDisplaySelfieModel.Factory _avatarDisplayModelFactory;
        [Inject] private CharacterManagerConfig _characterManagerConfig;
        
        private AvatarDisplaySelfieModel _avatarDisplayModel;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.AvatarPreview;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager manager)
        {    
            _backButton.onClick.AddListener(OpenInitiatorPage);
            _retakeButton.onClick.AddListener(OpenInitiatorPage);
            _confirmButton.onClick.AddListener(SaveCharacter);
        }

        protected override void OnDisplayStart(AvatarPreviewArgs args)
        {
            _backButton.interactable = false;
            _retakeButton.interactable = false;
            _confirmButton.interactable = false;
            
            base.OnDisplayStart(args);

            var genderId = OpenPageArgs.Gender.Id;
            _avatarDisplayModel = _avatarDisplayModelFactory.Create(new AvatarDisplaySelfieModel.Args
            {
                GenderId = genderId,
                Json = OpenPageArgs.Json
            });
            _avatarDisplayModel.AvatarReady += OnAvatarReady;
            
            _avatarDisplayView.Initialize(_avatarDisplayModel);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _avatarDisplayModel.AvatarReady -= OnAvatarReady;
            _avatarDisplayView.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnAvatarReady()
        {
            _backButton.interactable = true;
            _retakeButton.interactable = true;
            _confirmButton.interactable = true;
        }
        
        private void OpenInitiatorPage()
        {
            _avatarDisplayModel.CloseRoom();
            OpenPageArgs.OnBackButtonClicked?.Invoke();
        }

        private void SaveCharacter()
        {
            var currentCharacter = _avatarDisplayModel.GetCharacter();
            currentCharacter.GenderId = OpenPageArgs.Gender.Id;
            _avatarDisplayModel.CloseRoom();
            OpenPageArgs.OnCharacterConfirmed?.Invoke(currentCharacter);
        }
    }
}
