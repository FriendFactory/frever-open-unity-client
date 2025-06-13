using System;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing;
using StansAssets.Foundation.Async;
using Stateless;
using UnityEngine;
using Zenject;
using State = Modules.LevelManaging.Editing.AudioRecordingState;
using Trigger = UIManaging.Pages.LevelEditor.Ui.AudioRecordingTrigger;

namespace UIManaging.Pages.LevelEditor.Ui
{
    [UsedImplicitly]
    internal sealed class AudioRecordingStateController : IInitializable, IDisposable
    {
        private readonly AudioRecordingStateTransitionsProvider _transitionsProvider;
        private readonly IAudioRecordingStateHolder _audioRecordingStateHolder;
        
        private StateMachine<State, Trigger> _machine;
        private AudioRecordingState _previousState;
        private CancellationTokenSource _tokenSource;

        public State PreviousState => _previousState;
        public State State => _machine.State;
        
        public State DestinationState { get; private set; }
        public bool IsTransitioning { get; private set; }

        public event Action<State, State> TransitionStarted;
        public event Action<State, State> RecordingStateChanged;

        public AudioRecordingStateController(AudioRecordingStateTransitionsProvider transitionsProvider, IAudioRecordingStateHolder audioRecordingStateHolder)
        {
            _transitionsProvider = transitionsProvider;
            _audioRecordingStateHolder = audioRecordingStateHolder;
        }

        public void Initialize()
        {
            _tokenSource = new CancellationTokenSource();
            _machine = new StateMachine<State, Trigger>(State.None); 
            
            MainThreadDispatcher.Init();
            
            _machine.Configure(State.None)
                    .Permit(Trigger.ActiveVoice, State.Voice)
                    .Permit(Trigger.SelectMusic, State.MusicSelected);

            _machine.Configure(State.Voice)
                    .OnEntryAsync(async () => await EntryNextStateAsync(State.Voice))
                    .Permit(Trigger.ActivateMusic, State.MusicActivated)
                    .Permit(Trigger.SelectMusic, State.MusicSelected);
            
            _machine.Configure(State.MusicActivated)
                    .OnEntryAsync(async () => await EntryNextStateAsync(State.MusicActivated))
                    .Permit(Trigger.ActiveVoice, State.Voice)
                    .Permit(Trigger.SelectMusic, State.MusicSelected);

            _machine.Configure(State.MusicSelected)
                    .OnEntryFromAsync(AudioRecordingTrigger.StartMusicPreview, () => Task.CompletedTask)
                    .OnEntryAsync(async () => await EntryNextStateAsync(State.MusicSelected))
                    .Permit(Trigger.ActivateMusic, State.MusicActivated)
                    .Permit(Trigger.ActiveVoice, State.Voice)
                    .Permit(Trigger.StartMusicPreview, State.MusicPreviewed);

            _machine.Configure(State.MusicPreviewed)
                    .Permit(Trigger.StopMusicPreview, State.MusicSelected);
            
            _machine.OnTransitionCompleted(transition =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    IsTransitioning = false;
                    RecordingStateChanged?.Invoke(transition.Source, transition.Destination);
                    
                    _audioRecordingStateHolder.UpdateState(DestinationState);
                });
            });
            _machine.OnTransitioned(transition =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    IsTransitioning = true;
                    DestinationState = transition.Destination;
                    _previousState = transition.Source;
                    
                    TransitionStarted?.Invoke(transition.Source, transition.Destination);
                });
            });
        }
        
        public async void FireAsync(Trigger trigger)
        {
            if (!_machine.CanFire(trigger))
            {
                Debug.LogWarning($"{GetType().Name}] Can't fire {trigger} trigger # current state: {_machine.State}");
                return;
            }

            try
            {
               await _machine.FireAsync(trigger);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task EntryNextStateAsync(State destination)
        {
            if (!_transitionsProvider.TryGetTransition(_previousState, destination, out var transition))
            {
                Debug.LogWarning($"[{GetType().Name}] Can't find transition from {_previousState} to {destination}");
                return;
            }

            if (!transition.instant && transition.duration > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(transition.duration), _tokenSource.Token);
            }
        }

        public void Dispose()
        {
            _tokenSource?.CancelAndDispose();
        }
    }
}