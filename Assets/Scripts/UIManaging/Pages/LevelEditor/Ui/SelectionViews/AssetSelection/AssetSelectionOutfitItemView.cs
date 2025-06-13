using System;
using UIManaging.PopupSystem;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public sealed class AssetSelectionOutfitItemView : AssetSelectionAnimatedImageItemView
    {
        [Inject] private BaseEditorPageModel _pageModel;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        protected override void OnInitialized()
        {
            LevelManager.CharactersPositionsSwapped += RefreshState;
            LevelManager.EditingCharacterSequenceNumberChanged += RefreshState;
            RefreshState();
            base.OnInitialized();
        }

        protected override void BeforeCleanup()
        {
            LevelManager.CharactersPositionsSwapped -= RefreshState;
            LevelManager.EditingCharacterSequenceNumberChanged -= RefreshState;
            base.BeforeCleanup();
        }

        protected override void OnClicked()
        {
            if (State == ViewState.GreyOut)
            {
                var message = GetReasonWhyOutfitIsBlocked();
                _popupManagerHelper.ShowInformationMessage(message);
                return;
            }
            
            base.OnClicked();
        }

        private string GetReasonWhyOutfitIsBlocked()
        {
            string message = null;
            if (_pageModel.CanChangeOutfitForTargetCharacter(ref message))
            {
                throw new InvalidOperationException("Use can change the outfit");
            }
            return message;
        }
        
        private void RefreshState()
        {
            var state = _pageModel.CanChangeOutfitForTargetCharacter() ? ViewState.Active : ViewState.GreyOut;
            SetState(state);
        }
    }
}