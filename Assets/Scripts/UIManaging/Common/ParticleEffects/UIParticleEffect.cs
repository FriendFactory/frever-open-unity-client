using System.Collections;
using System.Linq;
using Coffee.UIExtensions;
using UnityEngine;

namespace UIManaging.Common.ParticleEffects
{
    [RequireComponent(typeof(UIParticle))]
    public sealed class UIParticleEffect : MonoBehaviour
    {
        [SerializeField] private UIParticleEffectSettings _settings; // Intrinsic state
        [SerializeField] private bool _playOnEnable = true;
        
        private UIParticle _uiParticle;
        private RectTransform _rectTransform;
        
        public UIParticleEffectSettings Settings => _settings;
        
        private UIParticle UIParticle => _uiParticle ? _uiParticle : (_uiParticle = GetComponent<UIParticle>());
        private RectTransform RectTransform => _rectTransform ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());

        public void Play()
        {
            UIParticle.StartEmission();
            UIParticle.Play();
        }
        
        public void Stop()
        {
            UIParticle.StopEmission();
            UIParticle.Stop();
        }

        // TODO: just a PoC, should be refactored
        public void SetSingleBurstCount(int count)
        {
            var ps = UIParticle.particles.First();
            var burst = ps.emission.GetBurst(0);
            
            burst.count = count;
            
            ps.emission.SetBurst(0, burst);
        }
        
        public void Attach(Transform parent, Vector3 position)
        {
            RectTransform.SetParent(parent, false);
            RectTransform.position = position;
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_uiParticle) return;

            _uiParticle = GetComponent<UIParticle>();
        }
    #endif

        private void OnEnable()
        {
            StartCoroutine(DespawnAfterDelay(_settings.DespawnDelay));

            if (_playOnEnable)
            {
                Play();
            }
        }
        
        private void OnDisable()
        {
            Stop();
        }

        private IEnumerator DespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            UIParticleEffectPool.ReturnToPool(this);
        }
    }
}