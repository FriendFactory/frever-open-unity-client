using DG.Tweening;
using UnityEngine;

namespace UIManaging.Pages.OnBoardingPage.UI
{
    public sealed class LoadingIndicator : MonoBehaviour
    {
        [SerializeField] private float _animationSpeed = 1.0f;
        [SerializeField] private Transform _icon;

        private Sequence _sequence;

        private void Awake()
        {
            _sequence = DOTween.Sequence();
            _sequence.Append(_icon
                            .DOLocalRotate(new Vector3(0, 0, -360), _animationSpeed, RotateMode.FastBeyond360)
                            .SetRelative(true)
                            .SetEase(Ease.Linear))
                     .SetLoops(-1);
                ;

        }

        private void OnEnable()
        {
            _sequence.Play();
        }

        private void OnDisable()
        {
            _sequence.Pause();
        }
    }
}