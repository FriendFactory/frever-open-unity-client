using DG.Tweening;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class DoTweenRotator : MonoBehaviour
    {
        [SerializeField] float _rotationSpeed = 200;

        private Tween _sequence;
        
        private void Awake()
        {
            _sequence = transform.DORotate(Vector3.back * 360f, _rotationSpeed, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetUpdate(true)
                .SetSpeedBased(true)
                .SetEase(Ease.Linear)
                .SetAutoKill(false);
        }

        private void OnEnable()
        {
            _sequence?.Restart();
        }

        private void OnDisable()
        {
            _sequence?.Pause();
        }
    }
}
