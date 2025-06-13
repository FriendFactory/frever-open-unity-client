using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    public class FadeInOutBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private FadeInBehaviourType _type;
        [SerializeField, ShowIf("_type", FadeInBehaviourType.Single)]
        private FadeInOutModel _model;
        [SerializeField, ShowIf("_type", FadeInBehaviourType.Multiple)]
        private List<FadeInOutModel> _models;

        private readonly List<Sequence> _sequences = new List<Sequence>();
        private int _activeSequences;
        
        public Action OnAnimationCompleted;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public List<FadeInOutModel> Models => _models;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnDisable()
        {
            CleanUp();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        [Button]
        public void FadeIn()
        {
            if (_type == FadeInBehaviourType.Single)
            {
                PlaySingleFadeInAnimation();
                return;
            }

            PlayFadeInAnimationGroup();
        }

        [Button]
        public void FadeOut()
        {
            if (_type == FadeInBehaviourType.Single)
            {
                PlaySingleFadeOutAnimation();
                return;
            }
            
            PlayFadeOutAnimationGroup();
        }

        public void CleanUp()
        {
            OnAnimationCompleted = null;
            
            _sequences.ForEach(s => s.Kill());
            _sequences.Clear();
            
            _model?.CleanUp();
            _models.ForEach(m => m.CleanUp());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PlaySingleFadeInAnimation()
        {
            var sequence = _model.ToInSequence(OnSimpleSequenceCompleted);
            sequence.Play();
            _sequences.Add(sequence);
        }

        private void PlayFadeInAnimationGroup()
        {
            foreach (var sequence in _models.Select(model => model.ToInSequence(OnGroupSequenceElementCompleted)))
            {
                _activeSequences++;
                sequence.Play();
                _sequences.Add(sequence);
            }
        }

        private void PlaySingleFadeOutAnimation()
        {
            var sequence = _model.ToOutSequence(OnSimpleSequenceCompleted);
            sequence.Play();
            _sequences.Add(sequence);
        }

        private void PlayFadeOutAnimationGroup()
        {
            foreach (var sequence in _models.Select(model => model.ToOutSequence(OnGroupSequenceElementCompleted)))
            {
                _activeSequences++;
                sequence.Play();
                _sequences.Add(sequence);
            }
        }

        private void OnSimpleSequenceCompleted(Sequence _)
        {
            OnAnimationCompleted?.Invoke();
            CleanUp();
        }

        private void OnGroupSequenceElementCompleted(Sequence sequence)
        {
            _activeSequences--;
            _sequences.Remove(sequence);

            if (_activeSequences != 0) return;
            
            OnAnimationCompleted?.Invoke();
            CleanUp();
        }

        private enum FadeInBehaviourType
        {
            Single = 0,
            Multiple = 1,
        }
    }
}