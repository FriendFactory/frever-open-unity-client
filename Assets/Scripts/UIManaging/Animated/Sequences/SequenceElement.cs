using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIManaging.Animated.Sequences
{
    [Serializable]
    public class SequenceElement : AnimationSequenceBase
    {
        [FormerlySerializedAs("type")] [SerializeField] internal AnimationType animationType;
        
        [ShowIf("animationType", AnimationType.Delay)]
        [SerializeField] private DelayAnimationSequence _delayAnimation = new DelayAnimationSequence();

        [ShowIf("animationType", AnimationType.Position)]
        [SerializeField] private AnimatePositionSequence _animatePosition = new AnimatePositionSequence();

        [ShowIf("animationType", AnimationType.Scale)]
        [SerializeField] private AnimateScaleSequence _animateScale = new AnimateScaleSequence();
        
        [ShowIf("animationType", AnimationType.Rotation)]
        [SerializeField] private AnimateRotationScale _animateRotation = new AnimateRotationScale();
        
        [ShowIf("animationType", AnimationType.Slider)]
        [SerializeField] private AnimateSliderSequence _animateSlider = new AnimateSliderSequence();
        
        [ShowIf("animationType", AnimationType.ImageFade)]
        [SerializeField] private AnimateFadeImageSequence _animateFadeImage = new AnimateFadeImageSequence();

        private GameObject _previewAnimatedObject;
        private Vector3 _previewPositionCache;
        private Vector3 _previewScaleCache;
        private Vector3 _previewRotationCache;

        public DelayAnimationSequence Delay => _delayAnimation;
        public AnimatePositionSequence Position => _animatePosition;
        public AnimateScaleSequence Scale => _animateScale;
        public AnimateRotationScale Rotation => _animateRotation;
        public AnimateSliderSequence Slider => _animateSlider;
        public AnimateFadeImageSequence FadeImage => _animateFadeImage;

        private Action<IAnimationSequence> _animationFinishedCallback;
        
        // ToDo: GetSequence by enum method
        public void Play(GameObject animatedObject, Action<IAnimationSequence> animationFinished)
        {
            _animationFinishedCallback = animationFinished;
            
            switch (animationType)
            {
                case AnimationType.Position:
                    _animatePosition.AnimationFinished += OnAnimationFinished;
                    _animatePosition.Play(animatedObject);
                    break;
                case AnimationType.Scale:
                    _animateScale.AnimationFinished += OnAnimationFinished;
                    _animateScale.Play(animatedObject);
                    break;
                case AnimationType.Rotation:
                    _animateRotation.AnimationFinished += OnAnimationFinished;
                    _animateRotation.Play(animatedObject);
                    break;
                case AnimationType.Delay:
                    _delayAnimation.AnimationFinished += OnAnimationFinished;
                    _delayAnimation.Play(animatedObject);
                    break;
                case AnimationType.Slider:
                    _animateSlider.AnimationFinished += OnAnimationFinished;
                    _animateSlider.Play(animatedObject);
                    break;
                case AnimationType.ImageFade:
                    _animateFadeImage.AnimationFinished += OnAnimationFinished;
                    _animateFadeImage.Play(animatedObject);
                    break;
                default:
                    throw new Exception($"Cannot animate object {animatedObject}: Unknown animation type {animationType}");
            }
        }

        internal void Preview(GameObject animatedObject)
        {
            _previewAnimatedObject = animatedObject;
            _previewPositionCache = animatedObject.transform.position;
            _previewScaleCache = animatedObject.transform.localScale;
            _previewRotationCache = animatedObject.transform.rotation.eulerAngles;
            
            Play(animatedObject, PostPreviewCleanUp);
            
        }

        private void PostPreviewCleanUp(IAnimationSequence animationSequence)
        {
            _previewAnimatedObject.transform.position = _previewPositionCache;
            _previewAnimatedObject.transform.localScale = _previewScaleCache;
            _previewAnimatedObject.transform.rotation = Quaternion.Euler(_previewRotationCache);

            animationSequence.AnimationFinished -= PostPreviewCleanUp;
        }

        private void OnAnimationFinished(IAnimationSequence animationSequence)
        {
            switch (animationType)
            {
                case AnimationType.Position:
                    _animatePosition.AnimationFinished -= OnAnimationFinished;
                    break;
                case AnimationType.Scale:
                    _animateScale.AnimationFinished -= OnAnimationFinished;
                    break;
                case AnimationType.Rotation:
                    _animateRotation.AnimationFinished -= OnAnimationFinished;
                    break;
                case AnimationType.Delay:
                    _delayAnimation.AnimationFinished -= OnAnimationFinished;
                    break;
                case AnimationType.Slider:
                    _animateSlider.AnimationFinished -= OnAnimationFinished;
                    break;
                case AnimationType.ImageFade:
                    _animateFadeImage.AnimationFinished -= OnAnimationFinished;
                    break;
            }
            
            _animationFinishedCallback?.Invoke(animationSequence);
        }
    }
}