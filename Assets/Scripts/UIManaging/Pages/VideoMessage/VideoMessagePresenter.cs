using Bridge;
using Extensions;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.Common.Files;
using Common;
using DigitalRubyShared;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Modules.LevelManaging.Editing.ThumbnailCreator;
using Modules.LevelViewPort;
using Navigation.Args;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading;
using UIManaging.Pages.VideoMessage.CharacterSizeManaging;
using UIManaging.Pages.VideoMessage.Emojis;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using IAsset = Modules.LevelManaging.Assets.IAsset;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.VideoMessage
{
    internal sealed class VideoMessagePresenter : MonoBehaviour
    {
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Button _addCaptionButton;
        [SerializeField] private Button _addMoreCaptionsButton;
        [SerializeField] private CaptionsPanel _captionsPanel;
        [SerializeField] private UploadingPanel _uploadingPanel;
        [SerializeField] private GameObject[] _objectsToHideOnCaptionShown;
        [SerializeField] private BackgroundSelectionPanel _backgroundSelectionPanel;
        [SerializeField] private EmotionSelectionPanel _emotionSelectionPanel;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _shareButton;
        [SerializeField] private LoadingOverlay _loadingOverlay;
        [SerializeField] private SwitchEditorOptionsPanel _switchEditorOptionsPanel;
        [SerializeField] private Button _openVideoOptionsPanelButton;
        [SerializeField] private CharacterSizeControllerVisibilityControl _characterSizeControllerVisibilityControl;
        [SerializeField] private CharacterSizeController _characterSizeController;
        [SerializeField] private CharacterViewProjection _characterViewProjection;
        [SerializeField] private RectTransform _characterViewPortMask;
        [SerializeField] private RectTransform _gestureArea;
        [SerializeField] private LevelViewPort _viewPort;
        [SerializeField] private CaptionPanelActivator _captionPanelActivator;
        
        [Inject] private ILevelManager _levelManager;
        [Inject] private IBridge _bridge;
        [Inject] private EventThumbnailsCreatorManager _eventThumbnailsCreatorManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private ICameraInputController _cameraInputController;
        [Inject] private IVideoMessagePageGesturesControl _videoMessagePageGesturesControl;
        [Inject] private FingersScript _fingersScript;
        [Inject] private PictureInPictureCameraControl _pictureCameraControl;
        [Inject] private CaptionProjectionManager _captionProjectionManager;
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private Action _onExitRequested;
        private Action<Level> _onLevelSaved;
        private Action _onLevelCreationRequested;
        private Action<NonLeveVideoData> _nonLevelVideoUploadRequested;

        private Level _originalLevel;
        private int _asyncCounter;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
            _addCaptionButton.onClick.AddListener(OnAddNewCaptionButtonClicked);
            _addMoreCaptionsButton.onClick.AddListener(OnAddNewCaptionButtonClicked);
            _exitButton.onClick.AddListener(OnExitClicked);
            _shareButton.onClick.AddListener(OnShareButtonPressed);
            _switchEditorOptionsPanel.EnterLevelEditorButtonClicked += OnEnterLevelEditorButtonClicked;
            _switchEditorOptionsPanel.GalleryVideoForUploadingPicked += RequestVideoFromGalleryUploading;
            _openVideoOptionsPanelButton.onClick.AddListener(_switchEditorOptionsPanel.Show);
            _fingersScript.PassThroughObjects.Add(_addCaptionButton.gameObject);
        }

        private void OnEnable()
        {
            _captionsPanel.CaptionPanelOpening += OnBeforeOpenCaptionPanel;
            _captionsPanel.Closed += OnCaptionPanelClosed;
        }

        private void OnDisable()
        {
            _captionsPanel.CaptionPanelOpening -= OnBeforeOpenCaptionPanel;
            _captionsPanel.Closed -= OnCaptionPanelClosed;
        }

        private void OnDestroy()
        {
            _fingersScript.PassThroughObjects.Remove(_addCaptionButton.gameObject);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public async Task Initialize(Level level, Action onExitRequested, Action<Level> onLevelSaved, Action onLevelCreationRequested, Action<NonLeveVideoData> nonLevelVideoRequested)
        {
            _originalLevel = await level.CloneAsync();
            _fingersScript.ResetState(true);
            InitializeEmotionSelection();
            
            _levelManager.SetFaceRecording(false);
            _levelManager.SetFaceTracking(false);
            
            //todo: drop, it should be cleared from the backend
            _levelManager.TargetEvent.CharacterController.First().GetCharacterControllerFaceVoiceController().FaceAnimation = null;
            _levelManager.TargetEvent.CharacterController.First().GetCharacterControllerFaceVoiceController().FaceAnimationId = null;
            
            _onExitRequested = onExitRequested;
            _onLevelSaved = onLevelSaved;
            _onLevelCreationRequested = onLevelCreationRequested;
            _nonLevelVideoUploadRequested = nonLevelVideoRequested;
            _cameraInputController.AutoActivateOnCameraSystemActivation = false;
            _switchEditorOptionsPanel.Init();
            _cameraInputController.Activate(false);
            var setLocationAsset = _levelManager.GetCurrentActiveSetLocationAsset();
            _pictureCameraControl.Init(setLocationAsset, _characterSizeController, _characterSizeController.BorderRectTransform, _viewPort.GetComponent<RectTransform>());
            _pictureCameraControl.Enable();
            _backgroundSelectionPanel.Init();
            _videoMessagePageGesturesControl.Enable(_gestureArea);
            _uploadingPanel.RefreshState(false);
            _viewPort.Init();
            SetupCharacterViewPortMask();
            
            _levelManager.ApplyRenderTextureToSetLocationCamera(_viewPort.RenderTexture);
            _characterViewProjection.Init(setLocationAsset, _levelManager.TargetCharacterAsset);
            _characterSizeControllerVisibilityControl.Init();
            _characterSizeControllerVisibilityControl.Run(true);
            await _captionsPanel.Init(_levelManager.GetActiveCamera());
            RefreshAddNewCaptionButtonState();
            _captionPanelActivator.Enable(true);
        }

        public void Cleanup()
        {
            var activeCamera = _levelManager.GetCurrentEventCamera();
            if (activeCamera != null)
            {
                activeCamera.targetTexture = null;
            }

            _pictureCameraControl.Cleanup();
            _videoMessagePageGesturesControl.Disable();
            _captionsPanel.Cleanup();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnSoundToggleChanged(bool isOn)
        {
            _levelManager.SetMusicVolume(isOn ? 100 : 0);
            _levelManager.SetVoiceVolume(0);
            PlayEvent();
        }

        private void InitializeEmotionSelection()
        {
            _emotionSelectionPanel.Init();
            _emotionSelectionPanel.EmotionSelected += OnEmojiItemSelected;
        }

        private async void OnEmojiItemSelected(long emotionId)
        {
            var resp = await _bridge.GetEmotionAssetsSetupAsync(emotionId);
            if (resp.IsError)
            {
                Debug.LogError(resp.ErrorMessage);
                return;
            }

            var assetsSetup = resp.Model;
            var animationId = assetsSetup.BodyAnimation.Id;
            var musicId = assetsSetup.Song.Id;
            var setAnimationTask = SetAnimationById(animationId);
            var setMusicTask = SetMusicById(musicId);
            await Task.WhenAll(setAnimationTask, setMusicTask);
        }

        private async Task SetAnimationById(long id)
        {
            _asyncCounter++;
            var bodyAnimation = await _bridge.GetBodyAnimationAsync(id);
            if (bodyAnimation == null)
            {
                _asyncCounter--;
                return;
            }
            _levelManager.ChangeBodyAnimation(bodyAnimation.Model, OnBodyAnimationChanged);

            void OnBodyAnimationChanged()
            {
                _asyncCounter--;
                PlayEventWhenAllLoaded();
            }
        }

        private async Task SetMusicById(long id)
        {
            _asyncCounter++;
            var songResult = await _bridge.GetSongsAsync(1, 0, songIds: new[] { id });
            if (songResult == null)
            {
                _asyncCounter--;
                return;
            }
            if (songResult.Models.Length == 0)
            {
                Debug.LogError($"Song with ID {id} not found!");
                return;
            }
            var song = songResult.Models[0];
            _levelManager.ChangeSong(song, onCompleted: OnSongChanged);

            void OnSongChanged(IAsset asset)
            {
                _asyncCounter--;
                OnSoundToggleChanged(_soundToggle.isOn);
                PlayEventWhenAllLoaded();
            }
        }

        private void PlayEventWhenAllLoaded()
        {
            if (_asyncCounter > 0) return;
            _cameraInputController.Activate(false);
            PlayEvent();
        }

        private void PlayEvent()
        {
            _levelManager.PlayEvent(PlayMode.PreviewLoop);
        }

        private void OnBeforeOpenCaptionPanel()
        { 
            _levelManager.StopCurrentPlayMode();
            _levelManager.PutEventOnFirstFrame();
            ChangeObjectsVisibility(false);
            _characterSizeControllerVisibilityControl.Stop();
            _pictureCameraControl.Disable();
        }

        private void OnCaptionPanelClosed()
        {
            ChangeObjectsVisibility(true);
            _levelManager.RefreshTargetEventAssets();
            if (_levelManager.TargetEvent.HasCaption())
            {
                foreach (var captionAsset in _levelManager.GetCurrentCaptionAssets())
                {
                    captionAsset.ForceRefresh();
                }
            }
            PlayEvent();
            _captionProjectionManager.SetupCaptionsProjection(_levelManager.TargetEvent.Caption);
            _characterSizeControllerVisibilityControl.Run(false);
            _pictureCameraControl.Enable();
            RefreshAddNewCaptionButtonState();
        }
        
        private void ChangeObjectsVisibility(bool show)
        {
            foreach (var item in _objectsToHideOnCaptionShown)
            {
                item.SetActive(show);
            }
        }

        private void OnExitClicked()
        {
            _onExitRequested?.Invoke();
        }

        private void OnShareButtonPressed()
        {
            var fileName = _levelManager.TargetEvent.Id.ToString();
            _eventThumbnailsCreatorManager.CaptureThumbnails(fileName, _levelManager.GetActiveCamera(), OnThumbnailCaptured);
        }
        
        private void OnThumbnailCaptured(FileInfo[] thumbnails)
        {
            _levelManager.TargetEvent.HasActualThumbnail = true;
            _levelManager.TargetEvent.Files = thumbnails.ToList();
            _loadingOverlay.Show(true);
            _levelManager.SaveLevel(_levelManager.CurrentLevel, OnSaved, OnSaveFailed);
        }
        
        private void OnSaveFailed(string message)
        {
            Debug.LogError($"Failed to save level. Reason: {message}");
            _loadingOverlay.Hide();
        }

        private void OnSaved(Level level)
        {
            _levelManager.CurrentLevel = level;
            _levelManager.ClearTempFiles();
            _loadingOverlay.Hide();
            _onLevelSaved?.Invoke(level);
        }
        
        private async void OnEnterLevelEditorButtonClicked()
        {
            _switchEditorOptionsPanel.SetEnterLevelEditorButtonInteractability(false);
            _switchEditorOptionsPanel.StopListeningToOutsideClickEvent();
            if (await _levelManager.IsLevelModified(_originalLevel, _levelManager.CurrentLevel))
            {
                _popupManagerHelper.ShowStashEditorChangesPopup(RequestSwitchingToLevelEditor,()=>
                {
                    _switchEditorOptionsPanel.StartListeningToOutsideClickEvent();
                    _switchEditorOptionsPanel.SetEnterLevelEditorButtonInteractability(true);
                });
            }
            else
            {
                RequestSwitchingToLevelEditor();
            }
        }

        private void RequestSwitchingToLevelEditor()
        {
            _levelManager.StopCurrentPlayMode();
            _characterSizeControllerVisibilityControl.Stop();
            _onLevelCreationRequested?.Invoke();
        }

        private void RequestVideoFromGalleryUploading(NonLeveVideoData videoData)
        {
            _levelManager.StopCurrentPlayMode();
            _characterSizeControllerVisibilityControl.Stop();
            _nonLevelVideoUploadRequested?.Invoke(videoData);
        }
        
        private void SetupCharacterViewPortMask()
        {
            _characterViewPortMask.CopyPosition(_viewPort.RectTransform);
            _characterViewPortMask.sizeDelta = _viewPort.RectTransform.sizeDelta;
        }

        private void OnAddNewCaptionButtonClicked()
        {
            var captionsCount = _levelManager.TargetEvent.GetCaptionsCount();
            if (captionsCount >= Constants.Captions.CAPTIONS_PER_EVENT_MAX)
            {
                _snackBarHelper.ShowFailSnackBar(Constants.Captions.REACHED_LIMIT_MESSAGE);
                return;
            }
            _captionsPanel.StartNewCaptionCreation();
        }

        private void RefreshAddNewCaptionButtonState()
        {
            _addCaptionButton.SetActive(_levelManager.TargetEvent.GetCaptionsCount() == 0);
        }
    }
}