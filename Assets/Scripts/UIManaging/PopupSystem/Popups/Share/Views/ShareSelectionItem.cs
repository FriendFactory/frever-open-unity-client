using Common.Abstract;
using TMPro;
using UIManaging.Common.SelectionPanel;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    public abstract class ShareSelectionItem<TModel> : BaseContextPanel<TModel>, IShareSelectionItem<TModel>
        where TModel : ShareSelectionItemModel
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private SelectionCheckmarkView _selectionCheckmarkView;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Inject] private PopupManager _popupManager;

        protected override bool IsReinitializable => true;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _nameText.text = ContextData.Title;
            RefreshPortraitImage();
            
            _selectionCheckmarkView.Initialize(ContextData);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = ContextData.IsLocked ? 0.5f : 1;
            }

            ContextData.SelectionChangeLocked += OnLockedClicked;
        }

        protected override void BeforeCleanUp()
        {
            ContextData.SelectionChangeLocked -= OnLockedClicked;
            
            _selectionCheckmarkView.CleanUp();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected abstract void RefreshPortraitImage();

        protected virtual void OnLockedClicked()
        {
            var config = new DirectMessagesLockedPopupConfiguration();
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType, true);
        }
    }
}