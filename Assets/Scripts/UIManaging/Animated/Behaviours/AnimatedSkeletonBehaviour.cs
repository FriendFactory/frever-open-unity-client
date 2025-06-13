using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class AnimatedSkeletonBehaviour : MonoBehaviour
    {
        [SerializeField] private List<UIGradient> _gradients;
        [SerializeField] private CanvasGroup _canvasGroup;
        private List<Sequence> _sequences = new List<Sequence>();

        public bool IsPlaying { get; private set; }

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            CleanUp();
        }

        [Button]
        public void CollectChildrenGradients()
        {
            _gradients.AddRange(GetComponentsInChildren<UIGradient>());
        }
        
        [Button]
        public void Play()
        {
            IsPlaying = true;
            
            FadeIn();
            
            foreach (var gradient in _gradients)
            {
                var sequence = DOTween.Sequence()
                                      .Append(DOVirtual.Float(1, -1, 1f, f => {
                                           if (!gradient)
                                           {
                                               CleanUp();
                                               return;
                                           }
                                           
                                           gradient.offset = f;
                                       }))
                                      .Append(DOVirtual.Float(-1, 1, 1f, f => {
                                           if (!gradient)
                                           {
                                               CleanUp();
                                               return;
                                           }
                                           
                                           gradient.offset = f;
                                       }))
                                      .SetLoops(-1);
                sequence.Play();
                _sequences.Add(sequence);
            }
        }

        public void FadeOut()
        {
            if (!IsPlaying) gameObject.SetActive(false);

            var sequence = DOTween.Sequence()
                                  .Append(_canvasGroup.DOFade(0, 0.25f))
                                  .OnComplete(CleanUpAfterFadeOut);
                                  
            _canvasGroup.blocksRaycasts = false;
            
            sequence.Play();
            IsPlaying = false;
        }
        
        public void FadeIn()
        {
            gameObject.SetActive(true);

            var sequence = DOTween.Sequence()
                                  .Append(_canvasGroup.DOFade(1, 0.25f));
            
            _canvasGroup.blocksRaycasts = true;
            
            sequence.Play();
        }

        public void CleanUp()
        {
            CleanUpSequences();
            
            IsPlaying = false;
        }

        private void CleanUpAfterFadeOut()
        {
            CleanUpSequences();
            
            gameObject.SetActive(false);
        }
        
        private void CleanUpSequences()
        {
            _sequences.ForEach(s => s?.Kill());
            _sequences.Clear();
        }
    }
}