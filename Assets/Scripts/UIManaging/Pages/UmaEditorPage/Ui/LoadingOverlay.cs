using System;
using System.Collections;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    internal sealed class LoadingOverlay: MonoBehaviour
    {
        private static readonly TimeSpan CANCEL_BUTTON_APPEAR_TIME = TimeSpan.FromSeconds(30);
        
        [SerializeField] private Button _cancelButton;
        private Action _onCancelButtonClicked;

        private void Awake()
        {
            _cancelButton.onClick.AddListener(()=> _onCancelButtonClicked?.Invoke());
        }

        public void Show(Action onCancelClicked)
        {
            _cancelButton.SetActive(false);
            gameObject.SetActive(true);
            _onCancelButtonClicked = onCancelClicked;
            StartCoroutine(ShowCancelButtonOnTimeout());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            StopCoroutine(ShowCancelButtonOnTimeout());
        }
        
        private IEnumerator ShowCancelButtonOnTimeout()
        {
            yield return new WaitForSeconds((float)CANCEL_BUTTON_APPEAR_TIME.TotalSeconds);
            _cancelButton.SetActive(true);
        }
    }
}