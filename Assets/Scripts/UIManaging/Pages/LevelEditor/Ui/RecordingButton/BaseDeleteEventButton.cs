using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    [RequireComponent(typeof(Button))]
    public abstract class BaseDeleteEventButton : MonoBehaviour
    {
        [Inject] protected ILevelManager LevelManager;
        private Button _button;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private Button Button
        {
            get
            {
                if (_button == null)
                {
                    _button = GetComponent<Button>();
                }

                return _button;
            }
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        protected virtual void OnEnable()
        {
            RefreshState();
            LevelManager.EventDeleted += OnEventDeleted;
            Button.onClick.AddListener(DeleteEvent);
            LevelManager.RecordingStarted += OnRecordingStarted;
            LevelManager.RecordingCancelled += OnRecordingCancelled;
            LevelManager.EventSaved += OnEventSaved;
        }

        protected virtual void OnDisable()
        {
            LevelManager.EventDeleted -= OnEventDeleted;
            Button.onClick.RemoveListener(DeleteEvent);
            LevelManager.RecordingStarted -= OnRecordingStarted;
            LevelManager.RecordingCancelled -= OnRecordingCancelled;
            LevelManager.EventSaved -= OnEventSaved;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        protected void RefreshState()
        {
            Button.interactable = CanButtonBeInteractable();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected virtual void DeleteEvent()
        {
            Button.interactable = false;
            LevelManager.UnloadFaceAndVoice();
        }

        protected abstract void OnEventDeleted();

        protected virtual bool CanButtonBeInteractable()
        {
            return !LevelManager.IsDeletingEvent && !LevelManager.IsLoadingAssets();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnEventSaved()
        {
            gameObject.SetActive(true);
        }

        private void OnRecordingCancelled()
        {
            gameObject.SetActive(LevelManager.CurrentLevel.Event.Count > 0);
        }

        private void OnRecordingStarted()
        {
            gameObject.SetActive(false);
        }
    }
}
