using Modules.VideoRecording;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    /// <summary>
    /// That is base button which prevents any actions from button on Share/Publish page
    /// during offline rendering and recording video to prevent any spike
    /// </summary>
    [RequireComponent(typeof(Button))]
    internal abstract class PublishPageButtonBase : MonoBehaviour
    {
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private IVideoRecorder _videoRecorder;
        [Inject] private IPublishVideoController _publishVideoController;

        private Button _button;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool Interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        private bool VideoCapturingRunning => _videoRecorder.IsCapturing;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClicked);
            _publishVideoController.RecordingCancelled += ActivateButton;
        }

        private void OnDestroy()
        {
            _publishVideoController.RecordingCancelled -= ActivateButton;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected abstract void OnButtonClicked();

        protected virtual bool IsActionAllowed()
        {
            return !VideoCapturingRunning;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnClicked()
        {
            if (IsActionAllowed())
            {
                Interactable = false;
                OnButtonClicked();
            }
            else
            {
                DisplayForbiddenActionPopup();
            }
        }

        private void DisplayForbiddenActionPopup()
        {
            _popupManagerHelper.OpenVideoStillRenderingPopup();
        }
        
        private void ActivateButton()
        {
            Interactable = true;
        }
    }
}