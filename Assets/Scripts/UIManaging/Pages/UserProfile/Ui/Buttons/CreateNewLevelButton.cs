using System;
using System.Collections.Generic;
using System.Linq;
using Modules.CharacterManagement;
using Modules.FeaturesOpening;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Core;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    [RequireComponent(typeof(Button))]
    internal sealed class CreateNewLevelButton : ButtonBase
    {
        [Inject] private CharacterManager _characterManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private PageManager _pageManager;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnClick()
        {
            if(_pageManager.CurrentPage.Id == PageId.CreatePost) return;
                
            if (_characterManager.SelectedCharacter == null)
            {
                _popupManagerHelper.OpenMainCharacterIsNotSelectedPopup();
                return;
            }

            this._pageManager.MoveNext(PageId.CreatePost, new CreatePostPageArgs());
        }
    }
}