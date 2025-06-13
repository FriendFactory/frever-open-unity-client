using Common;
using DG.Tweening;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.InputHandling;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.PostRecordEditorState;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class EditCameraPositionPanel : MonoBehaviour
    {
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _revertButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _previewButton;
        [SerializeField] private GameObject _infoBody;
        [SerializeField] private GameObject _editPanelBody;
        [SerializeField] private Image _previewIcon;
        [SerializeField] private Image _previewBackground;

        private PostRecordEditorPageModel _editorPageModel;
        private ICameraSystem _cameraSystem;
        private ICameraInputController _cameraInputController;
        private IInputManager _inputManager;
        private ILevelManager _levelManager;
        private ICameraTemplatesManager _cameraTemplatesManager;

        private bool _isPreviewing;
        private bool _infoHasBeenShown;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(PostRecordEditorPageModel editorPageModel, ICameraSystem cameraSystem,
            ICameraInputController cameraInputController, ILevelManager levelManager, ICameraTemplatesManager cameraTemplatesManager, IInputManager inputManager)
        {
            _editorPageModel = editorPageModel;
            _cameraSystem = cameraSystem;
            _cameraInputController = cameraInputController;
            _levelManager = levelManager;
            _cameraTemplatesManager = cameraTemplatesManager;
            _inputManager = inputManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _nextButton.onClick.AddListener(HideInfoPanel);
            _revertButton.onClick.AddListener(RevertChanges);
            _saveButton.onClick.AddListener(SaveChanges);
            _previewButton.onClick.AddListener(OnClickPreviewButton);
        }
        
        private void OnEnable()
        {
            _isPreviewing = false;
            EnableCameraInput();
            Display();
            FreezeOnEventFirstFrame();
        }
        
        private void OnDestroy()
        {
            _nextButton.onClick.RemoveListener(HideInfoPanel);
            _revertButton.onClick.RemoveListener(RevertChanges);
            _saveButton.onClick.RemoveListener(SaveChanges);
            _previewButton.onClick.RemoveListener(OnClickPreviewButton);
        }
        
        private void OnDisable()
        {
            DisableCameraInput();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void Display()
        {
            if (_infoHasBeenShown)
            {
                HideInfoPanel();
            }
            else
            {
                ShowInfoPanel();
            }
        }

        private void RevertChanges()
        {
            gameObject.SetActive(false);
            RevertCameraChanges();
            ReturnToPostRecordingEditor();
        }

        private void SaveChanges()
        {
            gameObject.SetActive(false);
            ReturnToPostRecordingEditor();
        }

        private void ShowInfoPanel()
        {
            _infoBody.SetActive(true);
            _editPanelBody.SetActive(false);
            _infoHasBeenShown = true;
        }

        private void HideInfoPanel()
        {
            _infoBody.SetActive(false);
            _editPanelBody.SetActive(true);
        }

        private void ReturnToPostRecordingEditor()
        {
            _editorPageModel.SetPostRecordEditorCameraTexture();
            _editorPageModel.PostRecordEditorSubscribeToEvents();
            _editorPageModel.ChangeState(AssetSelection);
        }

        private void OnClickPreviewButton()
        {
            if (_isPreviewing)
            {
                CancelPreview();
            }
            else
            {
                StartPreview();
            }
        }

        private void OnUserMovedCamera()
        {
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
        }

        private void ChangePreviewButtonColors(bool isPreviewing)
        {
            var endValue = 0;

            if (isPreviewing)
            {
                endValue = 1;
                _previewIcon.color = Color.black;
            }
            else
            {
                _previewIcon.color = Color.white;
            }

            const int duration = 0;
            _previewBackground.DOFade(endValue, duration);
        }

        private void StartPreview()
        {
            _isPreviewing = true;
            var targetEvent = _editorPageModel.PostRecordEventsTimelineModel.SelectedEvent.Event;
            ChangePreviewButtonColors(true);
            _levelManager.PlayEvent(PlayMode.PreviewWithCameraTemplate, targetEvent, null, OnPreviewComplete);
        }

        private void OnPreviewComplete()
        {
            _isPreviewing = false;
            ChangePreviewButtonColors(false);
            FreezeOnEventFirstFrame();
        }

        private void CancelPreview()
        {
            _levelManager.StopCurrentPlayMode();
            _isPreviewing = false;

            ChangePreviewButtonColors(false);
            FreezeOnEventFirstFrame();
        }

        private void FreezeOnEventFirstFrame()
        {
            var targetEvent = _editorPageModel.PostRecordEventsTimelineModel.SelectedEvent.Event;
            _levelManager.PlayEvent(PlayMode.PreviewWithCameraTemplate, targetEvent, Pause);

            void Pause()
            {
                CoroutineSource.Instance.ExecuteWithFrameDelay(PauseEvent);
            }
        }

        private void PauseEvent()
        {
            _levelManager.PauseEventPlayMode();
            _cameraSystem.PauseAnimation();
        }

        private void RevertCameraChanges()
        {
            var currentCameraAnimation = _levelManager.GetCurrentCameraAnimationAsset().Clip;
            var startFrame = currentCameraAnimation.GetFrame(0);
            _cameraTemplatesManager.SetStartFrameForTemplates(startFrame);
            var currentTemplateClip = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraTemplatesManager.UpdateStartPositionForTemplate(currentTemplateClip);
            _cameraSystem.Simulate(currentCameraAnimation, 0);
        }

        private void EnableCameraInput()
        {
            _cameraInputController.Activate(true);
            _inputManager.Enable(true, GestureType.Pan, GestureType.Zoom);
            _cameraInputController.CameraModificationCompleted += OnUserMovedCamera;
        }

        private void DisableCameraInput()
        {
            _cameraInputController.Activate(false);
            _inputManager.Enable(false, GestureType.Pan, GestureType.Zoom);
            _cameraInputController.CameraModificationCompleted -= OnUserMovedCamera;
        }
    }
}