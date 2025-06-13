using DG.Tweening;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    internal abstract class CharacterFocusButtonBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float GROW_DURATION = 0.3f;
        private readonly Vector3 _growScale = new Vector3(1.1f, 1.1f, 1f);
        
        [SerializeField] protected Image _thumbnail;
        [SerializeField] protected RawImage BackgroundImage;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _selectedColor = new Color(0.698f, 0.553f, 0.945f, 1f);

        [Inject] private LevelEditorCameraSettingsLocalization _localization;

        private RectTransform _rectTransform;
        private Image _image;
        private Tween _growTween;
        private Tween _recordCircleTween;
        private UnityAction _onClick;
        private UnityAction _onRecordingStart;
        private UnityAction _onHoldRecordingStart;
        private UnityAction _onHoldRecordingStop;
        private ScrollRect _scrollRectParent;
        private int _currentPointerId;
        private bool _isGracePeriodRunning;

        protected ILevelManager LevelManager;
        protected ICameraSystem CameraSystem;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public int PositionId { get; set; }
        public abstract int TargetSequenceNumber { get; }
        public bool IsSelected { get; private set; }
        public Image Thumbnail => _thumbnail;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        protected virtual void OnDestroy()
        {
            _onClick = null;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public virtual void Initialize(ILevelManager levelManager, ICameraSystem cameraSystem)
        {
            LevelManager = levelManager;
            CameraSystem = cameraSystem;
            _image = GetComponent<Image>();
        }

        public abstract void FocusOnTarget();

        public void SetInteractable(bool interactable)
        {
            _image.raycastTarget = interactable;
        }

        public void AddOnClickListener(UnityAction action)
        {
            _onClick += action;
        }

        public void RemoveOnClickListener(UnityAction action)
        {
            _onClick -= action;
        }

        public void OnClick()
        {
            _onClick?.Invoke();
        }

        public virtual void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;

            if (IsSelected)
            {
                PlayGrowTween();
            }
            else
            {
                PlayShrinkTween();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _currentPointerId = eventData.pointerId;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_currentPointerId != eventData.pointerId) return;
            _onClick?.Invoke();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PlayGrowTween()
        {
            _growTween?.Kill();
            BackgroundImage.color = _selectedColor;
            _growTween = RectTransform.DOScale(_growScale, GROW_DURATION);
        }

        private void PlayShrinkTween()
        {
            _growTween?.Kill();
            BackgroundImage.color = _defaultColor;
            _growTween = RectTransform.DOScale(Vector3.one, GROW_DURATION);
        }
    }
}