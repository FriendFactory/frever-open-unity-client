using Abstract;
using DG.Tweening;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif 

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal abstract class AudioRecordingPanelTweenAnimation: MonoBehaviour, IInitializable
    {
        [SerializeField] protected float _duration = 0.5f;
        [SerializeField] private Ease _ease = Ease.InOutSine;
        [SerializeField] protected string _tweenId;
        [SerializeField] protected bool _warmUpOnInitialize = false;

        public string TweenId => _tweenId;
        public bool IsInitialized { get; private set; }
        public ExtendedSequence Sequence => _sequence;
        
        private ExtendedSequence _sequence;
        
        public virtual void Initialize()
        {
            _sequence = new ExtendedSequence(GetSequence());

            if (_warmUpOnInitialize)
            {
                _sequence.Complete();
                _sequence.Rewind();
            }

            IsInitialized = true;
        }

        public Sequence GetSequence()
        {
            var sequence = BuildSequence().SetId(TweenId).SetAutoKill(false).SetEase(_ease).Pause();

            return sequence;
        }

        public virtual void CleanUp()
        {
            _sequence?.Dispose();

            IsInitialized = false;
        }

        public void Play(bool forward = true, bool instant = false)
        {
            if (forward)
            {
                _sequence.PlayForward(instant);
            }
            else
            {
                _sequence.PlayBackwards(instant);
            }
        }
        
        protected abstract Sequence BuildSequence();
        
        #if UNITY_EDITOR
        
        [Button]
        private void PlayForward()
        {
            Play(true);
        }

        [Button]
        private void PlayBackward()
        {
            Play(false);
        }

        [Button]
        private void PlayForwardInstant()
        {
            Play(true, true);
        }
        
        [Button]
        private void PlayBackwardInstant()
        {
            Play(false, true);
        }

        #endif
        
    }
}