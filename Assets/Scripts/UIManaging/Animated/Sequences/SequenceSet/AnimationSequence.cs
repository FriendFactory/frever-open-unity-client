using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIManaging.Animated.Sequences
{
    [Serializable]
    public class AnimationSequence
    {
        [SerializeField] private SequenceType type;
        [SerializeField] private List<SequenceElement> _sequences = null;
        
        [Header("Debug")]
        [SerializeField] private int _sequenceIndex;
        
        private GameObject _animatedObject;
        private Action _animationFinished;

        internal SequenceElement this[int i] => _sequences[i];

        public AnimationSequence(SequenceType sequenceType)
        {
            type = sequenceType;
            _sequences = new List<SequenceElement>();
        }
        
        public SequenceElement GetAnimationElementAt(int index)
        {
            return _sequences[index];
        }
        
        public void Play(GameObject animatedObject, Action onAnimationFinished = null)
        {
            _animatedObject = animatedObject;
            _animationFinished = onAnimationFinished;
            if (type == SequenceType.Parallel)
            {
                PlayInParallel();
                return;
            }
            
            PlayInSequence();
        }

        internal void Add(SequenceElement sequenceElement)
        {
            if (_sequences == null)
            {
                _sequences = new List<SequenceElement>();
            }
            
            _sequences.Add(sequenceElement);
        }
        
        private void PlayInParallel()
        {
            _sequenceIndex = _sequences.Count;
            foreach (var element in _sequences)
            {
                element.Play(_animatedObject, OnParallelElementFinished);
            }
        }

        private void PlayInSequence()
        {
            _sequenceIndex = 0;
            PlaySequenceElement();
        }

        private void PlaySequenceElement()
        {
            if (_sequences.Count - 1 < _sequenceIndex)
            { 
                _animationFinished?.Invoke();
                return;
            }

            PlayNext();
        }

        private void PlayNext()
        {
            _sequences[_sequenceIndex].Play(_animatedObject, OnSequenceElementFinished);
            _sequenceIndex++;
        }

        private void OnSequenceElementFinished(IAnimationSequence animationSequence)
        {
            animationSequence.AnimationFinished -= OnSequenceElementFinished;
            PlaySequenceElement();
        }

        private void OnParallelElementFinished(IAnimationSequence animationSequence)
        {
            _sequenceIndex--;
            
            if(_sequenceIndex == 0) _animationFinished.Invoke();
        }
        
        private void SequenceElementFinished()
        {
            
        }

        private void OnAnimationFinished()
        {
            _animationFinished?.Invoke();
        }
    }
}