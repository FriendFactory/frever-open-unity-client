using Extensions;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    internal sealed class DeleteTargetEventButton : BaseDeleteEventButton
    {
        [Inject] private PopupManager _popupManager;
        [Inject] private LevelEditorPopupLocalization _levelEditorPopupLocalization;

        private long _targetEventId;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            LevelManager.StartUpdatingAsset += OnStartUpdatingAsset;
            LevelManager.StopUpdatingAsset += OnStopUpdatingAsset;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LevelManager.StartUpdatingAsset -= OnStartUpdatingAsset;
            LevelManager.StopUpdatingAsset -= OnStopUpdatingAsset;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetTargetEvent(long eventId)
        {
            _targetEventId = eventId;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void DeleteEvent()
        {
            ShowDeletePopup();
        }

        protected override void OnEventDeleted()
        {
            RefreshState();
        }

        protected override bool CanButtonBeInteractable()
        {
            if (LevelManager.CurrentLevel == null)
            {
                return false;
            }
            
            var hasMoreThanOneEvent = LevelManager.CurrentLevel.Event.Count > 1;
            return base.CanButtonBeInteractable() && hasMoreThanOneEvent;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnStopUpdatingAsset(DbModelType type, long id)
        {
            RefreshState();
        }

        private void OnStartUpdatingAsset(DbModelType type, long id)
        {
            RefreshState();
        }

        private void ConfirmDeletion()
        {
            base.DeleteEvent();
            LevelManager.DeleteEvent(_targetEventId);
            OnEventDeleted();
        }

        private void CancelDeletion()
        {
            RefreshState();
        }

        private void ShowDeletePopup()
        {
            var configuration = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDarkV3,
                Title = _levelEditorPopupLocalization.DeleteClipPopupDescription,
                YesButtonText = _levelEditorPopupLocalization.DeleteClipConfirmButton,
                NoButtonText = _levelEditorPopupLocalization.DeleteClipCancelButton,
                OnYes = ConfirmDeletion,
                OnNo = CancelDeletion,
                OnClose = (x)=> CancelDeletion(),
                YesButtonSetTextColorRed = true
            };
                
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }
    }
}
