using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Animated.Sequences
{
    public static class SequenceElementHelper
    {
        public static AnimationSequence NewAnimation(SequenceType sequenceType)
        {
            return new AnimationSequence(sequenceType);
        }

        public static AnimationSequence AnimateSlider(this AnimationSequence element, 
            Slider animatedSlider,
            AnimateSliderModel model = null, 
            Action<IAnimationSequence> onSequenceFinished = null)
        {
            var slider = SliderSequence(model);

            if (onSequenceFinished != null)
            {
                slider.AnimationFinished += onSequenceFinished;
            }

            slider.Slider._animatedSlider = animatedSlider;
            element.Add(slider);
            
            return element;
        }

        public static AnimationSequence AnimatePosition(this AnimationSequence element, AnimatePositionModel? model = null)
        {
            var position = PositionSequence();
            position.Position.Initialize(model);
            element.Add(position);

            return element;
        }

        public static AnimationSequence AnimateScale(this AnimationSequence element, Vector3 from, Vector3 to, float time, Action callback = null)
        {
            var scale = ScaleSequence();
            scale.Scale.Initialize(from, to, time);
            scale.Scale.AnimationFinished += _ => callback?.Invoke();
            
            element.Add(scale);
            
            return element;
        }
        
        public static AnimationSequence FadeImage(this AnimationSequence element, float from, float to, float time)
        {
            var fade = FadeImageSequence();
            fade.FadeImage.Initialize(from, to, time);
            element.Add(fade);

            return element;
        }
        
        public static SequenceElement SliderSequence(AnimateSliderModel animateSliderModel = null)
        {
            var element = new SequenceElement
            {
                animationType = AnimationType.Slider
            };

            if (animateSliderModel != null)
            {
                element.Slider.SetModel(animateSliderModel);
            }

            return element;
        }

        private static SequenceElement PositionSequence()
        {
            return new SequenceElement
            {
                animationType = AnimationType.Position,
            };
        }

        private static SequenceElement ScaleSequence()
        {
            return new SequenceElement
            {
                animationType = AnimationType.Scale,
            };
        }

        private static SequenceElement FadeImageSequence()
        {
            return new SequenceElement
            {
                animationType = AnimationType.ImageFade,
            };
        }
    }
}