using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class DoTweenLoadingSpinner : MonoBehaviour
    {
        [SerializeField] private float _animationSpeed = 1;
        [SerializeField] private float _rotationSpeed = 200;
        [SerializeField] private Transform _imageTransform;
        [SerializeField] private Image _radialImage;
        [SerializeField] private float _fillFrom = 0.1f;
        [SerializeField] private float _fillTo = 0.8f;
        [SerializeField] private float _fillRotationSpeedScale = 2f;
        
        private Tween _rotationSequence;
        private Tween _timeScaleSequence;
        private Tween _animationSequence;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _animationSequence = _radialImage.DOFillAmount(_fillTo, _animationSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);
            _rotationSequence = _imageTransform.DORotate(Vector3.back * 360f, _rotationSpeed, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetSpeedBased(true)
                .SetEase(Ease.Linear);
            _timeScaleSequence = _rotationSequence.DOTimeScale(_fillRotationSpeedScale, _animationSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);
            
            _rotationSequence.Pause();
            _animationSequence.Pause();
            _timeScaleSequence.Pause();
        }

        private void OnEnable()
        {
            _radialImage.fillAmount = _fillFrom;
            _radialImage.fillClockwise = false;
            _animationSequence.Restart();
            _rotationSequence.Restart();
            _timeScaleSequence.Restart();
        }

        private void OnDisable()
        {
            _animationSequence.Pause();
            _rotationSequence.Pause();
            _timeScaleSequence.Pause();
        }

        private void OnDestroy()
        {
            _animationSequence.Kill();
            _rotationSequence.Kill();
            _timeScaleSequence.Kill();
        }
    }
}