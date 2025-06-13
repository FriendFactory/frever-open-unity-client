using System;
using DG.Tweening;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    [RequireComponent(typeof(Button))]
    internal sealed class DeleteLastEventButton : MonoBehaviour
    {
        [SerializeField] private RecordButtonBlocker _recordButtonBlocker;
        [SerializeField] private LevelDurationProgressUI levelDurationProgressUI;
        [Inject] private ILevelManager _levelManager;

        private CanvasGroup _canvasGroup;
        private Button _button;
        private CanvasGroup CanvasGroup => _canvasGroup == null ? _canvasGroup = GetComponent<CanvasGroup>() : _canvasGroup;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(DeleteEvent);
        }

        private void OnDestroy()
        {
            CanvasGroup.DOKill();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show()
        {
            ShowImmediately();

            levelDurationProgressUI.PlayDeleteLastEventAnimation();
        }
        
        public void Hide(Action onComplete = null)
        {
            HideImmediately();

            levelDurationProgressUI.StopDeleteLastEventAnimation();

            onComplete?.Invoke();
        }

        private void ShowImmediately()
        {
            gameObject.SetActive(true);
            CanvasGroup.DOKill();
            CanvasGroup.alpha = 1f;
        }

        public void HideImmediately()
        {
            gameObject.SetActive(false);
            CanvasGroup.DOKill();
            CanvasGroup.alpha = 0f;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DeleteEvent()
        {
            _recordButtonBlocker.Switch(true);
            _levelManager.EventDeleted += OnEventDeleted;
            _button.interactable = false;
            _levelManager.DeleteLastEvent();
        }

        private void OnEventDeleted()
        {
            _button.interactable = true;
            _levelManager.EventLoadingCompleted -= OnEventDeleted;
        }
    }
}