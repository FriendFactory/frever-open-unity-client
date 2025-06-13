using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    [RequireComponent(typeof(Button))]
    internal sealed class FocusPanelToggleButton : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Sprite _defaultBackground;
        [SerializeField] private Sprite _collapsedBackground;
        [Space]
        [SerializeField] private GameObject _thumbnailContainer;
        [SerializeField] private Image _defaultThumbnail;
        [SerializeField] private Image _customThumbnail;

        [Inject] private ILevelManager _levelManager;

        private RectTransform _rectTransform;
        private Button _button;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

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

        private Button Button
        {
            get
            {
                if (_button == null)
                {
                    _button = GetComponent<Button>();
                }

                return _button;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetListener(UnityAction action)
        {
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(action);
        }

        public void UpdateThumbnail(Sprite sprite)
        {
            if (sprite != null)
            {
                _defaultThumbnail.gameObject.SetActive(false);
                _customThumbnail.gameObject.SetActive(true);
                _customThumbnail.sprite = sprite;
            }
            else
            {
                _defaultThumbnail.gameObject.SetActive(true);
                _customThumbnail.gameObject.SetActive(false);
            }
        }

        public void SetBackground(bool isCollapsed)
        {
            _backgroundImage.sprite = isCollapsed ? _defaultBackground : _collapsedBackground;
            _thumbnailContainer.SetActive(isCollapsed);
        }
    }
}