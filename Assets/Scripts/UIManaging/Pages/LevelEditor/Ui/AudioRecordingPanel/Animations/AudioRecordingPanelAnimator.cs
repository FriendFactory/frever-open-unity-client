using System;
using System.Collections.Generic;
using System.Linq;
using Common.Abstract;
using Extensions;
using Modules.LevelManaging.Editing;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class AudioRecordingPanelAnimator: BaseContextlessPanel 
    {
        [SerializeField] private AudioRecordingStateTransitionsProvider _transitionsProvider;
        [SerializeField] private List<AudioRecordingPanelTweenAnimation> _animations;

        [Inject] private AudioRecordingStateController _stateController;
        
        private Dictionary<(AudioRecordingState, AudioRecordingState), Action> _sequencesStateMap;

        protected override void OnInitialized()
        {
            InitializeTweens();
            
            _stateController.TransitionStarted += OnTransitioned;
        }

        protected override void BeforeCleanUp()
        {
            _stateController.RecordingStateChanged -= OnTransitioned;
            
            _animations.Where(tween => tween.IsInitialized).ForEach(tween => tween.CleanUp());
        }
        
        private void OnTransitioned(AudioRecordingState source, AudioRecordingState destination)
        {
            if (_sequencesStateMap.TryGetValue((source, destination), out var action))
            {
                action?.Invoke();
            }
        }

        private void InitializeTweens()
        {
            _sequencesStateMap = new Dictionary<(AudioRecordingState, AudioRecordingState), Action>();
            
            GenerateSequencesToStatesMap();
        }

        private void GenerateSequencesToStatesMap()
        {
            _transitionsProvider.Transitions.ForEach(transition =>
            {
                var source= transition.source;
                var destination = transition.destination;
                
                if (string.IsNullOrEmpty(transition.tweenId)) return;
                
                var targetAnimation = _animations.FirstOrDefault(tweenAnimation => tweenAnimation.TweenId == transition.tweenId);
                
                if (targetAnimation == null) return;
                
                _sequencesStateMap.Add((source, destination), () =>
                {
                    if (!targetAnimation.IsInitialized)
                    {
                        targetAnimation.Initialize();
                    }
                    
                    var sequence = targetAnimation.Sequence;
                    if (transition.forward)
                    {
                        sequence.PlayForward(transition.instant);
                    }
                    else
                    {
                        sequence.PlayBackwards(transition.instant);
                    }
                });
            });
        }
    }
}