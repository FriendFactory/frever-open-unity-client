using System;
using UIManaging.Animated.Sequences;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    public sealed class AnimatedLevelIconBehaviour : SequenceAnimationController
    {
        public event Action PlayFireworks;
        
        internal override void BuildSequence()
        {
            var small = new Vector3(0.8f, 0.8f, 0.9f);
            var large = new Vector3(1.1f, 1.1f, 1.1f);
            Animation = SequenceElementHelper.NewAnimation(SequenceType.Sequence)
                                             .FadeImage(0.0f, 1.0f, 0.001f)
                                             .AnimateScale(Vector3.one, large, 0.117f)
                                             .AnimateScale(large, small, 0.167f, PlayFireworks)
                                             .AnimateScale(small, Vector3.one, 0.1f)
                                             .FadeImage(1.0f, 0.0f, 1.0f);
        }
    }
}