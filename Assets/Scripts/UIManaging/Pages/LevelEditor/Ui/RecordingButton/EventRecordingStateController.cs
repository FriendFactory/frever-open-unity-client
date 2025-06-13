using System;
using Extensions;
using Modules.FaceAndVoice.Face.Facade;
using Modules.LevelManaging.Editing.LevelManagement;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    /// <summary>
    /// Here should be controlling of EventRecordButton state
    /// </summary>
    internal sealed class EventRecordingStateController
    {
        public event Action<bool> StateUpdated;
        
        private readonly ILevelManager _levelManager;
        private readonly IArSessionManager _arSession;
        private bool _isSubscribedToEvents;

        public EventRecordingStateController(ILevelManager levelManager, IArSessionManager arSession)
        {
            _levelManager = levelManager;
            _arSession = arSession;
        }

        public void StartControl()
        {
            StartListenEvents();
            UpdateState();
        }

        private void StartListenEvents()
        {
            if(_isSubscribedToEvents) return;
            _isSubscribedToEvents = true;
            
            _levelManager.EventDeleted += UpdateState;
            _levelManager.EventSaved += UpdateState;
            _arSession.StateSwitched += OnArSessionSwitched;
        }

        private void OnArSessionSwitched(bool isOn)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            var levelHasSpaceForNewEvent = _levelManager.LevelDurationSeconds <= _levelManager.MaxLevelDurationSec - _levelManager.MinEventDurationMs.ToSeconds();
            StateUpdated?.Invoke(levelHasSpaceForNewEvent);
        }

        public void StopControl()
        {
            StopListenEvents();
        }
        
        private void StopListenEvents()
        {
            if(!_isSubscribedToEvents) return;
            _isSubscribedToEvents = false;
            
            _levelManager.EventDeleted -= UpdateState;
            _levelManager.EventSaved -= UpdateState;
            _arSession.StateSwitched -= OnArSessionSwitched;
        }
    }
}
