using System;
using System.Collections.Generic;
using I2.Loc;
using Modules.PhotoBooth.Profile;
using Navigation.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    internal abstract class BaseProfilePhotoPreviewablePage<TArgs> : GenericPage<TArgs>
        where TArgs : BaseProfilePhotoPreviewPageArgs
    {
        [Header("UI Components")] 
        [SerializeField] protected Button _forwardButton;
        [SerializeField] private PhotoPreviewController _photoPreviewController;
        [Header("Localization")]
        [SerializeField] private LocalizedString _profileButtonName;
        [SerializeField] private LocalizedString _backgroundButtonName;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _forwardButton.onClick.RemoveListener(OnForwardButtonPressed);
        }

        //---------------------------------------------------------------------
        // Page 
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _forwardButton.onClick.AddListener(OnForwardButtonPressed);
        }

        protected override void OnDisplayStart(TArgs args)
        {
            base.OnDisplayStart(args);
            
            ChangeButtonName(args.PhotoType);

            _photoPreviewController.Initialize(args);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);

            _photoPreviewController.CleanUp();
        }
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------

        protected abstract void OnForwardButtonPressed();

        protected virtual void OnBackButtonPressed()
        {
            Manager.MoveBack();
        }
        
        private void ChangeButtonName(ProfilePhotoType photoType)
        {
            var buttonName = _forwardButton.GetComponentInChildren<TMP_Text>();
            if (buttonName == null) return;

            switch (photoType)
            {
                case ProfilePhotoType.Profile:
                    buttonName.text = _profileButtonName;
                    break;
                case ProfilePhotoType.Background:
                    buttonName.text = _backgroundButtonName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(photoType), photoType, null);
            }
        }
    }
}