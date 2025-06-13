using System.Linq;
using System.Threading;
using Bridge;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.EditingPage;
using UIManaging.Pages.LevelEditor.Ui.FeatureControls;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class LastEventPreviewButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private CanvasGroup _body;
        [SerializeField] private EditingPageLoading _loadingScreen;
        [SerializeField] private LevelEditorPageUI _pageUiControl;

        private ILevelManager _levelManager;
        private ICameraSystem _cameraSystem;
        private ICameraInputController _cameraInputController;
        private AmplitudeManager _amplitudeManager;
        private ICameraTemplatesManager _cameraTemplatesManager;
        private CancellationTokenSource _cancellationTokenSource;
        private IPreviewLastEventFeatureControl _featureControl;
        
        private Event LastEvent => _levelManager.CurrentLevel.Event.LastOrDefault();

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private bool Interactable
        {
            set => _button.interactable = value;
        }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager, ICameraInputController cameraInputController,
            ICameraSystem cameraSystem, AmplitudeManager amplitudeManager,
            ICameraTemplatesManager cameraTemplatesManager, IPreviewLastEventFeatureControl control)
        {
            _levelManager = levelManager;
            _cameraInputController = cameraInputController;
            _cameraSystem = cameraSystem;
            _amplitudeManager = amplitudeManager;
            _cameraTemplatesManager = cameraTemplatesManager;
            _featureControl = control;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Init()
        {
            _button.onClick.AddListener(OnClick);

            _levelManager.RecordingStarted += Hide;
            _levelManager.RecordingCancelled += OnEventRecordingCancelled;
            _levelManager.EventSaved += OnEventSaved;
            _levelManager.EventStarted += OnEventStarted;
            _levelManager.EventDeletionStarted += OnEventDeletionStarted;
            _levelManager.RecordingEnded += OnRecordingEnded;
        }

        public void CleanUp()
        {
            _cancellationTokenSource?.Cancel();
            _button.onClick.RemoveListener(OnClick);

            _levelManager.RecordingStarted -= Hide;
            _levelManager.RecordingCancelled -= OnEventRecordingCancelled;
            _levelManager.EventSaved -= OnEventSaved;
            _levelManager.EventStarted -= OnEventStarted;
            _levelManager.EventDeletionStarted -= OnEventDeletionStarted;
            _levelManager.RecordingEnded -= OnRecordingEnded;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnClick()
        {
            _cameraInputController.Activate(false);
            _loadingScreen.ShowDark("Loading preview...");
            _levelManager.PreviewCancelled += BackToRecordingMode;
            _levelManager.EditingCharacterSequenceNumber = LastEvent.TargetCharacterSequenceNumber;
            _levelManager.UnloadNotUsedByEventsAssets(LastEvent);
            _levelManager.PlayEventPreview(LastEvent, OnLastEventStarted, BackToRecordingMode);
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.LAST_EVENT_PREVIEW_BUTTON_CLICKED);
        }

        private void OnLastEventStarted()
        {
            _loadingScreen.Hide(0f);
        }
        
        private void BackToRecordingMode()
        {
            _cameraInputController.Activate(true);
            _levelManager.PreviewCancelled -= BackToRecordingMode;
            
            ForceCameraToLastAnimationFramePosition();
            UseLastPreviewFrameAsCameraTemplateStartPosition();
            
            _levelManager.PrepareNewEventBasedOnTarget();
            _pageUiControl.SetupSelectedItems(_levelManager.TargetEvent);
            _levelManager.PlayEvent(PlayMode.PreRecording);
        }

        private void ForceCameraToLastAnimationFramePosition()
        {
            var cameraAnim = _levelManager.GetCurrentCameraAnimationAsset();
            _cameraSystem.Simulate(cameraAnim.Clip, 0);
        }
        
        private void UseLastPreviewFrameAsCameraTemplateStartPosition()
        {
            var lastAnimFrame = _levelManager.GetCurrentCameraAnimationLastFrame();
            _cameraTemplatesManager.SetStartFrameForTemplates(lastAnimFrame);
        }

        private void OnEventSaved()
        {
            Display();
        }

        private void OnEventStarted()
        {
            RefreshInteractivity();

            if (_levelManager.IsLevelEmpty)
            {
                Hide();
            }
            else
            {
                Display();
            }
        }

        private void OnEventRecordingCancelled()
        {
            if (_levelManager.IsLevelEmpty)
            {
                Hide();
            }
            else
            {
                Display();
            }
        }

        private void OnEventDeletionStarted()
        {
            Interactable = false;
        }

        private void OnEventDeleted()
        {
            Interactable = true;
        }

        private void Hide()
        {
            Cleanup();
            _body.Disable();
        }

        private void Display()
        {
            _body.Enable();
            _levelManager.EventDeleted -= OnEventDeleted;
            _levelManager.EventDeleted += OnEventDeleted;
        }

        private void Cleanup()
        {
            _levelManager.EventDeleted -= OnEventDeleted;
        }
        
        private void OnRecordingEnded()
        {
            Interactable = true;
        }

        private void RefreshInteractivity()
        {
            Interactable = !_levelManager.IsLevelEmpty && _featureControl.CanPreviewEvent(_levelManager.GetLastEvent());
        }
    }
}