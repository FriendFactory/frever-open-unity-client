using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIManaging.Pages.VideoMessage.Emojis
{
    internal sealed class EmojiUIItem : EnhancedScrollerCellView
    {
        [SerializeField] private GameObject _selectionGameObject;
        [SerializeField] private TMP_Text _emojiText;

        private EmojiUiItemModel _model;

        private Button _button;

        void OnEnable()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        public void Setup(EmojiUiItemModel model)
        {
            _model = model;
            _emojiText.text = model.Emoji.EmojiCode;
            RefreshSelectionState();
        }

        public void RefreshSelectionState()
        {
            _selectionGameObject.SetActive(_model.IsSelected);
        }

        private void OnItemSelected()
        {
            _model.OnClick(_model.Emoji);
        }

        private void OnClick()
        {
            OnItemSelected();
        }
    }
}
