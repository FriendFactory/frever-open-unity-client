using DG.Tweening;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class TextVisibilityButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Sprite _iconEnabled;
        [SerializeField] private Sprite _iconDisabled;
        [SerializeField] private TMP_Text[] _labels;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private  void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void Reset()
        {
            _button = GetComponentInChildren<Button>();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ChangeStateSilently(bool isActive)
        {
            _icon.sprite = isActive ? _iconEnabled : _iconDisabled;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnClicked()
        {
            var isActive = _labels[0].gameObject.activeSelf;

            _icon.sprite = isActive ? _iconDisabled : _iconEnabled;

            foreach (var label in _labels)
            {
                label.DOKill();
                label.SetActive(!isActive);
                label.alpha = 1f;
            }
        }
    }
}