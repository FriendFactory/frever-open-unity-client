using DG.Tweening;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class ExceptionPopup : InformationPopup<ExceptionPopupConfiguration>
    {
        private const float EXPAND_SIZE = 600f;
        private const float EXPAND_DURATION = 0.1f;

        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _expandButton;
        [SerializeField] private Button _copyMessageButton;
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private RectTransform _scrollViewContentRect;
        [SerializeField] private GameObject _extendedMessagePanel;

        private readonly Vector3 _expandButtonRotation = new Vector3(-180f, 0, 0);
        private readonly TextEditor _textEditor = new TextEditor();

        private Vector3 _originSizeDelta;
        private RectTransform _descriptionTextRect;

        //---------------------------------------------------------------------
        // Mono
        //---------------------------------------------------------------------

        private void Awake()
        {
            _quitButton.onClick.AddListener(OnQuitButtonClicked);
#if DEVELOPMENT_BUILD || EXCEPTION_MESSAGE_EXPAND_ENABLED
            _expandButton.onClick.AddListener(OnExpandButtonClicked);
            _copyMessageButton.onClick.AddListener(OnCopyMessageButtonClicked);
            
            _descriptionTextRect = _descriptionText.GetComponent<RectTransform>();
#else
            _expandButton.gameObject.SetActive(false);
#endif
        }

        private void OnDestroy()
        {
            _quitButton.onClick.RemoveListener(OnQuitButtonClicked);
#if DEVELOPMENT_BUILD || EXCEPTION_MESSAGE_EXPAND_ENABLED
            _expandButton.onClick.RemoveListener(OnExpandButtonClicked);
            _copyMessageButton.onClick.RemoveListener(OnCopyMessageButtonClicked);
#endif
        }

        //---------------------------------------------------------------------
        // BasePopup
        //---------------------------------------------------------------------

        protected override void OnConfigure(ExceptionPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);
            
            _originSizeDelta = _panelRect.sizeDelta;
            _extendedMessagePanel.SetActive(false);
        }


        //---------------------------------------------------------------------
        // Button Callbacks
        //---------------------------------------------------------------------

        private void OnQuitButtonClicked()
        {
            _descriptionText.text = string.Empty;
            if (_extendedMessagePanel.activeSelf) InstantCollapse();
            Hide();
        }

        private void OnExpandButtonClicked()
        {
            if (_extendedMessagePanel.activeSelf)
            {
                Collapse();
            }
            else
            {
                Expand();
            }
        }

        private void OnCopyMessageButtonClicked()
        {
            CopyMessage();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Expand()
        {
            _expandButton.interactable = false;
            _extendedMessagePanel.SetActive(true);
            var sizeDelta = _panelRect.sizeDelta;
            _expandButton.transform.DORotate(_expandButtonRotation, 0);
            _panelRect.DOSizeDelta(new Vector2(sizeDelta.x, sizeDelta.y + EXPAND_SIZE),
                EXPAND_DURATION).OnComplete(OnExpandComplete);

            void OnExpandComplete()
            {
                _expandButton.interactable = true;
                var textSizeDelta = _descriptionTextRect.sizeDelta;
                _scrollViewContentRect.sizeDelta = new Vector2(textSizeDelta.x, textSizeDelta.y);
            }
        }

        private void Collapse()
        {
            _expandButton.interactable = false;
            _extendedMessagePanel.SetActive(false);
            var sizeDelta = _panelRect.sizeDelta;
            _expandButton.transform.DORotate(Vector3.zero, 0);
            _panelRect.DOSizeDelta(new Vector2(sizeDelta.x, sizeDelta.y - EXPAND_SIZE),
                EXPAND_DURATION).OnComplete(() => { _expandButton.interactable = true; });
        }

        private void InstantCollapse()
        {
            _expandButton.interactable = true;
            _extendedMessagePanel.SetActive(false);
            _panelRect.sizeDelta = _originSizeDelta;
            _expandButton.transform.DORotate(Vector3.zero, 0);
        }

        private void CopyMessage()
        {
            _textEditor.text = _descriptionText.text;
            _textEditor.SelectAll();
            _textEditor.Copy();
        }
    }
}