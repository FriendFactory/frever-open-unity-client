using System;
using DG.Tweening;
using UnityEngine;

namespace UIManaging.Animated.Sequences
{
    [Serializable]
    // Should be an abstract object but Unity's serialization won't allow it.
    public class AnimationSequenceBase : IAnimationSequence
    {
        public event Action<IAnimationSequence> AnimationFinished;

        public virtual void Play(GameObject animatedObject)
        {
            throw new NotImplementedException();
        }

        public virtual Sequence Build()
        {
            throw new NotImplementedException();
        }

        public virtual void OnSequenceFinished()
        {
            OnAnimationFinished();
        }

        protected void OnAnimationFinished()
        {
            AnimationFinished?.Invoke(this);
            AnimationFinished = null;
        }
    }
}