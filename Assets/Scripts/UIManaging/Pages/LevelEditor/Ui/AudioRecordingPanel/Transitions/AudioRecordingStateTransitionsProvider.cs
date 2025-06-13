using System.Collections.Generic;
using System.Linq;
using Modules.LevelManaging.Editing;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    [CreateAssetMenu(fileName = "AudioRecordingStateTransitionsProvider", menuName = "Friend Factory/Audio Recording State Transitions Provider")]
    internal sealed class AudioRecordingStateTransitionsProvider: ScriptableObject, IInitializable
    {
        [SerializeField] private List<AudioRecordingStateTransition> _transitions;
        
        public List<AudioRecordingStateTransition> Transitions => _transitions;
        
        private Dictionary<(AudioRecordingState, AudioRecordingState), AudioRecordingStateTransition> _transitionMap;

        public void Initialize()
        {
            _transitionMap = _transitions.ToDictionary(t => (t.source, t.destination));
        }
        
        public bool TryGetTransition(AudioRecordingState source, AudioRecordingState destination, out AudioRecordingStateTransition transition)
        {
            if (_transitionMap == null) Initialize();
            
            transition = _transitions.FirstOrDefault(t => t.source == source && t.destination == destination);

            return transition != null;
        }
    }
}