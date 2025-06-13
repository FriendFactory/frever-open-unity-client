using Modules.InputHandling;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Players;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Preview
{
    internal sealed class PreviewButtonsHolder: MonoBehaviour
    {
        [SerializeField] private Button _cancelPreviewButton;
        
        [Inject] private ILevelManager _levelManager;
        [Inject] private IBackButtonEventHandler _backButtonEventHandler;

        private bool _allowPreviewCancellation;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(bool allowPreviewCancellation)
        {
            _allowPreviewCancellation = allowPreviewCancellation;
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _levelManager.LevelPreviewStarted += OnPreviewStarted;
            _levelManager.LevelPreviewCompleted += OnPreviewCompleted;
            _levelManager.EventPreviewCompleted += OnPreviewCompleted;
            _levelManager.PreviewCancelled += OnPreviewCancelled;
            _levelManager.EventLoadingStarted += OnEventLoadingStarted;
            _levelManager.EventLoadingCompleted += OnEventLoadingCompleted;

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _levelManager.LevelPreviewStarted -= OnPreviewStarted;
            _levelManager.LevelPreviewCompleted -= OnPreviewCompleted;
            _levelManager.EventPreviewCompleted -= OnPreviewCompleted;
            _levelManager.PreviewCancelled -= OnPreviewCancelled;
            _levelManager.EventLoadingStarted -= OnEventLoadingStarted;
            _levelManager.EventLoadingCompleted -= OnEventLoadingCompleted;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnPreviewStarted()
        {
            gameObject.SetActive(_allowPreviewCancellation);

            if (_allowPreviewCancellation)
            {
                _cancelPreviewButton.onClick.AddListener(OnPreviewCancelClick);
                _backButtonEventHandler.AddButton(_cancelPreviewButton.gameObject, () => _cancelPreviewButton.onClick?.Invoke());
            }
        }

        private void OnPreviewCompleted()
        {
            gameObject.SetActive(false);
            _cancelPreviewButton.onClick.RemoveAllListeners();
            _backButtonEventHandler.RemoveButton(_cancelPreviewButton.gameObject);
        }

        private void OnPreviewCancelled()
        {
            gameObject.SetActive(false);
            _cancelPreviewButton.onClick.RemoveAllListeners();
            _backButtonEventHandler.RemoveButton(_cancelPreviewButton.gameObject);
        }

        private void OnEventLoadingStarted()
        {
            SetInteractable(false);
        }

        private void OnEventLoadingCompleted()
        {
            SetInteractable(true);
        }

        private void OnPreviewCancelClick()
        {
            _levelManager.CancelPreview(PreviewCleanMode.KeepFirstEvent);
        }

        private void SetInteractable(bool enable)
        {
            _cancelPreviewButton.interactable = enable;
        }
    }
}