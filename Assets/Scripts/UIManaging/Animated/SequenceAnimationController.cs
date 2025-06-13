using System;
using Sirenix.OdinInspector;
using UIManaging.Animated.Sequences;
using UnityEngine;

namespace UIManaging.Animated
{
    public abstract class SequenceAnimationController : MonoBehaviour
    {
        [SerializeField] private GameObject _animatedObject;
        [SerializeField] protected AnimationSequence Animation;

        public virtual void Reset()
        {
            BuildSequence();
        }

        public void Play(Action onAnimationFinished)
        {
            Animation.Play(_animatedObject, onAnimationFinished);
        }

        [Button("Rebuild Sequence")]
        internal abstract void BuildSequence();
    }
}