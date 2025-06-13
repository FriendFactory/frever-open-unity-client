using System;
using UnityEngine;

namespace UIManaging.Animated.Sequences
{
    public interface IAnimationSequence
    {
        event Action<IAnimationSequence> AnimationFinished;
        
        void Play(GameObject animatedObject);
    }
}