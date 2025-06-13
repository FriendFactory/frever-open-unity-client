using System.Collections.Generic;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.ThumbnailCreator;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    internal sealed class EventRecordingUIManager : MonoBehaviour
    {
        [SerializeField] private LevelDurationProgressUI buttonProgressUI;
        [SerializeField] private RecordButtonBlocker _recordButtonBlocker;
        [SerializeField] private List<GameObject> _uiObjectsToDisable = new List<GameObject>();

        private EventRecordingService _eventRecordingService;
        private ILevelManager _levelManager;
        private EventThumbnailsCreatorManager _eventThumbnailsCreatorManager;
        private float _levelLengthSec;
        

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(EventRecordingService eventRecordingService, ILevelManager levelManager, 
            EventThumbnailsCreatorManager eventThumbnailsCreatorManager)
        {
            _eventRecordingService = eventRecordingService;
            _levelManager = levelManager;
            _eventThumbnailsCreatorManager = eventThumbnailsCreatorManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            buttonProgressUI.Setup();

            _eventRecordingService.RecordingStarted += OnRecordingStarted;
            _eventRecordingService.RecordingEnded += OnRecordingStopped;
            _eventRecordingService.RecordingCancelled += OnRecordingCancelled;
        }

        private void OnEnable()
        {
            _recordButtonBlocker.Switch(_levelManager.IsLoadingAssets());
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnSubscribeEvents();
        }

        public void Initialize()
        {
            buttonProgressUI.SetProgress(0);
        }

        private void SubscribeEvents()
        {
            _levelManager.AssetUpdateStarted += OnAssetChangingStarted;
            _levelManager.AssetUpdateCancelled += OnAssetUpdatingCancelled;
            _levelManager.AssetUpdateCompleted += OnAssetUpdated;
            _levelManager.EventDeleted += HideRecordButtonBlocker; 
            _levelManager.EventSaved += HideRecordButtonBlocker;
            _eventRecordingService.RecordTick += OnRecordTick; 
        }
        
        private void UnSubscribeEvents()
        {
            _levelManager.AssetUpdateStarted -= OnAssetChangingStarted;
            _levelManager.AssetUpdateCancelled -= OnAssetUpdatingCancelled;
            _levelManager.AssetUpdateCompleted -= OnAssetUpdated;
            _levelManager.EventDeleted -= HideRecordButtonBlocker;
            _levelManager.EventSaved -= HideRecordButtonBlocker;
            _eventRecordingService.RecordTick -= OnRecordTick; 
        }

        private void OnRecordTick(float recordingTime)
        {
            var progress = (_levelLengthSec + recordingTime) / _levelManager.MaxLevelDurationSec;
            buttonProgressUI.SetProgress(progress);
        }

        private void OnDestroy()
        {
            _eventRecordingService.RecordingStarted -= OnRecordingStarted;
            _eventRecordingService.RecordingEnded -= OnRecordingStopped;
            _eventRecordingService.RecordingCancelled -= OnRecordingCancelled;
            
            buttonProgressUI.UnsubscribeEvents();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnRecordingStarted()
        {
            _levelLengthSec = _levelManager.LevelDurationSec;
            SwitchUi(false);
            var setLocation = _levelManager.GetTargetEventSetLocationAsset();
            if (setLocation == null) return;

            var screenshotCamera = setLocation.Camera;

            if (screenshotCamera == null) return;

            _eventThumbnailsCreatorManager.ThumbnailsCaptured += OnThumbnailCaptured;
        }

        private void OnThumbnailCaptured()
        {
            _eventThumbnailsCreatorManager.ThumbnailsCaptured -= OnThumbnailCaptured;
        }

        private void OnRecordingStopped()
        {
            SwitchUi(true);
        }

        private void OnRecordingCancelled()
        {
            SwitchUi(true);
            var amount = _levelManager.LevelDurationSeconds / _levelManager.MaxLevelDurationSec;
            buttonProgressUI.SetProgress(amount);
        }

        private void SwitchUi(bool isActive)
        {
            foreach (var uiObject in _uiObjectsToDisable)
            {
                uiObject.SetActive(isActive);
            }
        }
        
        private void OnAssetChangingStarted(DbModelType type, long id)
        {
            _recordButtonBlocker.Switch(true);
        }

        private void OnAssetUpdatingCancelled(DbModelType type)
        {
            HideBlockerIfNeeded();
        }
        
        private void OnAssetUpdated(DbModelType type)
        {
            HideBlockerIfNeeded();
        }

        private void HideBlockerIfNeeded()
        {
            if (!_levelManager.IsDeletingEvent && !_levelManager.IsLoadingAssets())
            {
                HideRecordButtonBlocker();
            }
        }

        private void HideRecordButtonBlocker()
        {
            _recordButtonBlocker.Switch(false);
        }
    }
}