using Extensions;
using Modules.LevelManaging.Assets;
using UIManaging.Pages.LevelEditor.Ui.AssetButtons;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    internal sealed class CharacterOverlay: MonoBehaviour
    {
        [SerializeField] private CharacterAnimationButton _button;
        [SerializeField] private RectTransform _icon;
        [SerializeField] private RectTransform _arrow;
        [Space]
        [SerializeField] private Vector3 _hipOffset;
        [SerializeField] private float _noScaleDistance;
        [SerializeField] private float _sizeThreshold;

        private ISetLocationAsset _setLocationAsset;
        private ICharacterAsset _characterAsset;
        private RectTransform _rectTransform;
        private RectTransform _renderViewPort;
        private RectTransform _parentRectTransform;

        private float _parentPortHalfWidth;
        private float _parentPorHalfHeight;

        private float _overlayHalfWidth;
        private float _overlayHalfHeight;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public float Distance { get; private set; }

        private RectTransform RectTransform
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
        // Public
        //---------------------------------------------------------------------
        
        public void Init(ISetLocationAsset setLocationAsset, ICharacterAsset characterAsset, int sequenceNumber, RectTransform renderViewPort)
        {
            _characterAsset = characterAsset;
            _characterAsset.Destroyed += OnCharacterDestroyed;

            _setLocationAsset = setLocationAsset;
            _setLocationAsset.Destroyed += OnLocationAssetDestroyed;

            _renderViewPort = renderViewPort;

            _button.Init(sequenceNumber);

            _parentRectTransform = (RectTransform) transform.parent;

            _parentPortHalfWidth = _parentRectTransform.rect.width / 2f;
            _parentPorHalfHeight = _parentRectTransform.rect.height / 2f;

            _overlayHalfWidth = RectTransform.rect.width / 2f;
            _overlayHalfHeight = RectTransform.rect.height / 2f;
        }
        
        public void CleanUp()
        {
            if (_characterAsset == null) return;
            
            _characterAsset.Destroyed -= OnCharacterDestroyed;
            _setLocationAsset.Destroyed -= OnLocationAssetDestroyed;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        public void Update()
        {
            var currentCamera = _setLocationAsset.Camera;

            var fow = currentCamera.fieldOfView;
            var targetWorldPos = _characterAsset.HipTransform.position + _hipOffset;
            Distance = Vector3.Distance(currentCamera.transform.position, targetWorldPos);

            var realHeightSize = Distance * (2f * Mathf.Tan( Mathf.Deg2Rad * fow / 2f));
            _icon.localScale = Vector3.one * Mathf.Clamp01(1f - ((realHeightSize - _noScaleDistance) / _sizeThreshold));

            var targetViewportPos = currentCamera.WorldToViewportPoint(targetWorldPos);
            var worldNormalizedPos = _renderViewPort.GetWorldPositionFromNormalized(targetViewportPos);
            var screenPos = RectTransformUtility.WorldToScreenPoint(null, worldNormalizedPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, screenPos, null, out var localPos);

            localPos.x = Mathf.Clamp(localPos.x, _overlayHalfWidth - _parentPortHalfWidth, _parentPortHalfWidth - _overlayHalfWidth);
            localPos.y = Mathf.Clamp(localPos.y, _overlayHalfHeight - _parentPorHalfHeight, _parentPorHalfHeight - _overlayHalfHeight);

            RectTransform.localPosition = localPos;

            UpdateArrow(localPos);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateArrow(Vector2 localPos)
        {
            if (localPos.x <= _overlayHalfWidth - _parentPortHalfWidth || localPos.x >= _parentPortHalfWidth - _overlayHalfWidth ||
                localPos.y <= _overlayHalfHeight - _parentPorHalfHeight || localPos.y >= _parentPorHalfHeight - _overlayHalfHeight)
            {
                _arrow.gameObject.SetActive(true);
                _arrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-localPos.x, localPos.y) * Mathf.Rad2Deg);

            }
            else
            {
                _arrow.gameObject.SetActive(false);
            }
        }

        private void OnCharacterDestroyed(long id)
        {
            if (_characterAsset.Id != id) return;

            _characterAsset.Destroyed -= OnCharacterDestroyed;
            this.SetActive(false);
        }

        private void OnLocationAssetDestroyed(long id)
        {
            if (_setLocationAsset.Id != id) return;

            _setLocationAsset.Destroyed -= OnLocationAssetDestroyed;
            this.SetActive(false);
        }
    }
}