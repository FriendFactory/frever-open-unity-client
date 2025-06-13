using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class ShuffleText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _subtitleText;
        [Space]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _opaqueDuration = 4f;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private float _showDelay = 0.5f;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDisable()
        {
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(string title, string subtitle)
        {
            gameObject.SetActive(true);

            _titleText.text = title;
            _subtitleText.text = subtitle;

            _canvasGroup.DOKill();
            _canvasGroup.DOFade(1, _fadeDuration).SetDelay(_showDelay);

            _canvasGroup.DOFade(0, _fadeDuration)
                        .SetDelay(_showDelay + _opaqueDuration)
                        .OnComplete(OnComplete);

            void OnComplete()
            {
                gameObject.SetActive(false);
            }
        }
    }
}