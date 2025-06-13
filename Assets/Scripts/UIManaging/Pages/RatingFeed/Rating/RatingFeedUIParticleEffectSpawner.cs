using Extensions;
using JetBrains.Annotations;
using UIManaging.Common.ParticleEffects;
using UIManaging.Pages.RatingFeed.Signals;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class RatingFeedUIParticleEffectSpawner: MonoBehaviour
    {
        [SerializeField] private UIParticleEffectSettings _effectSettings;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 100f);
        
        private SignalBus _signalBus;

        
        [Inject, UsedImplicitly]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            
            _signalBus.Subscribe<ScoreAnimationFinishedSignal>(OnScoreAnimationFinished);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<ScoreAnimationFinishedSignal>(OnScoreAnimationFinished);
        }

        private void OnScoreAnimationFinished(ScoreAnimationFinishedSignal signal)
        {
            var particleCount = signal.Score * 5;
            var anchorPosition = signal.TargetPosition;
            
            SpawnEffect(particleCount, anchorPosition);
        }

        private void SpawnEffect(int particleCount, Vector3 anchorPosition)
        {
            var effect = UIParticleEffectPool.Spawn(_effectSettings);
            var position = anchorPosition + _offset;
            
            effect.Attach(transform, position);
            effect.SetSingleBurstCount(particleCount);
            effect.Play();
        }
    }
}