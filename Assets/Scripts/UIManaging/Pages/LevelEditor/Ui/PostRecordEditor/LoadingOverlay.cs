using DG.Tweening;
using Extensions;
using Modules.InputHandling;
using TMPro;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    public class LoadingOverlay : MonoBehaviour
    {
        [SerializeField] private float _fadeSpeed = 3f;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _counter;
        [SerializeField] private TextMeshProUGUI _title;
        
        [Inject] private IBackButtonEventHandler _backButtonEventHandler;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDisable()
        {
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0f;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(bool withCounter = false, string title = "")
        {
            gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = true;
            _counter.SetActive(withCounter);
            _title.text = title;
            _title.SetActive(!string.IsNullOrEmpty(title));
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(1f, _fadeSpeed).SetSpeedBased().SetUpdate(true);
            
            _backButtonEventHandler.ProcessEvents(false);
        }

        public void Hide()
        {
            _counter.SetActive(false);
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0f, _fadeSpeed).SetSpeedBased().SetUpdate(true).OnComplete(() => gameObject.SetActive(false));
            
            _backButtonEventHandler.ProcessEvents(true);
        }
    }
}