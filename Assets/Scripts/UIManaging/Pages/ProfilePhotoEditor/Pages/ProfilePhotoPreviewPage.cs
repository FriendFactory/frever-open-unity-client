using System;
using Extensions;
using Modules.InputHandling;
using Navigation.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    internal sealed class ProfilePhotoPreviewPage : BaseProfilePhotoPreviewablePage<ProfilePhotoPreviewPageArgs>, IPointerClickHandler
    {
        [Inject] private IBackButtonEventHandler _backButtonEventHandler;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override PageId Id => PageId.ProfilePhotoPreview;

        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------
        
        public void OnPointerClick(PointerEventData eventData)
        {
            var mousePoint = Input.mousePosition;
            var rectToIgnore = _forwardButton.GetComponent<RectTransform>();

            if (!RectTransformUtility.RectangleContainsScreenPoint(rectToIgnore, mousePoint))
            {
                OnBackButtonPressed();
            }
        }
        
        //---------------------------------------------------------------------
        // Page 
        //---------------------------------------------------------------------
        
        protected override void OnDisplayStart(ProfilePhotoPreviewPageArgs args)
        {
            var isLocalUser = args.Profile != null;
            _forwardButton.SetActive(isLocalUser);
            
            base.OnDisplayStart(args);
            
            _backButtonEventHandler.AddButton(gameObject, OnBackButtonPressed);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            _backButtonEventHandler.RemoveButton(gameObject);
        }

        protected override void OnForwardButtonPressed()
        {
            var args = new ProfilePhotoEditorPageArgs()
            {
                Profile = OpenPageArgs.Profile,
                PhotoType = OpenPageArgs.PhotoType,
                OnConfirmBackPageId = PageId.UserProfile
            };

            Manager.MoveNext(PageId.ProfilePhotoEditor, args, false);
        }
    }
}