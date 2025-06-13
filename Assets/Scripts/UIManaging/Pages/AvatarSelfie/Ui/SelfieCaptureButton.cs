using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SelfieCaptureButton : MonoBehaviour
{
    [SerializeField] private float _faceShapeAnimDuration = 0.3f;
    [SerializeField] private float _animDuration = 0.5f;
    [SerializeField] private float _fillSegmentTo = 0.25f;
    [SerializeField] private float _rotationSpeed = 4f;
    [SerializeField] private Color _activatedColor;
    [SerializeField] private Color _deactivatedColor = Color.white;

    [SerializeField] private Button _button;
    [SerializeField] private Image _buttonBody;
    [SerializeField] private Transform _spinnerParent;
    [SerializeField] private Image[] _spinnerSegmentsGroup;
    [SerializeField] private Image _faceShape;
    
    private Tween _spinningSequence;
    private Sequence _enabledSequence;
    private Sequence _faceShapeSequence;
    private bool _state;
    
    //---------------------------------------------------------------------
    // Messages
    //---------------------------------------------------------------------
    
    protected void Awake()
    {
        _spinningSequence = transform.DORotate(Vector3.back * 360f, _rotationSpeed, RotateMode.FastBeyond360)
            .SetLoops(-1)
            .SetUpdate(true)
            .SetEase(Ease.Linear)
            .SetAutoKill(false);

        _enabledSequence = DOTween.Sequence();
        _enabledSequence.SetAutoKill(false);
        _enabledSequence.Join(_spinnerParent.DOScale(1f, _animDuration).SetEase(Ease.InOutCubic));
        _enabledSequence.Join(_buttonBody.transform.DOScale(1f, _animDuration).SetEase(Ease.InOutCubic));
        _enabledSequence.Join(_buttonBody.DOColor(_activatedColor, _animDuration).SetEase(Ease.InOutCubic));
        _faceShapeSequence = DOTween.Sequence();
        _faceShapeSequence.SetAutoKill(false);
        _faceShapeSequence.Join(_faceShape.transform.DOScale(1f, _faceShapeAnimDuration).SetEase(Ease.InOutCubic));
        _faceShapeSequence.Join(_faceShape.DOColor(_activatedColor, _faceShapeAnimDuration).SetEase(Ease.InOutCubic));

        for (var i = 0; i < _spinnerSegmentsGroup.Length; i++)
        {
            var segment = _spinnerSegmentsGroup[i];
            _enabledSequence.Join(segment.DOColor(_deactivatedColor, _animDuration).SetEase(Ease.Linear));

            if (i != 0 && i != _spinnerSegmentsGroup.Length / 2 - 1)
            {
                _enabledSequence.Join(segment.DOFillAmount(_fillSegmentTo, _animDuration).SetEase(Ease.Linear));
            }
        }

        _enabledSequence.Pause();
#if UNITY_EDITOR
        _button.interactable = true;
#else
        _button.interactable = false;
#endif
    }

    private void OnEnable()
    {
        _spinningSequence?.Restart();
        _enabledSequence?.Restart();
        _faceShapeSequence?.Restart();
        _faceShapeSequence?.Pause();
        _enabledSequence?.Pause();
    }

    private void OnDisable()
    {
        _spinningSequence?.Pause();
        _enabledSequence?.Pause();
        _faceShapeSequence?.Pause();
    }
    
    private void OnDestroy()
    {
        _spinningSequence?.Kill();
        _enabledSequence?.Kill();
        _faceShapeSequence?.Kill();
    }
    
    //---------------------------------------------------------------------
    // Public
    //---------------------------------------------------------------------
    
    public void SetState(bool activated)
    {
        if (_state == activated) return;
        _state = activated;
#if UNITY_EDITOR
        _button.interactable = true;
#else
        _button.interactable = _state;
#endif
        
        if (_state)
        {
            _faceShapeSequence.PlayForward();
            _enabledSequence.PlayForward();
        }
        else
        {
            _faceShapeSequence.PlayBackwards();
            _enabledSequence.PlayBackwards();
        }
    }
}
