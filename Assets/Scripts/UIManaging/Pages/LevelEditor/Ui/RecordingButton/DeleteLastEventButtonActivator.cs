using System;
using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    [RequireComponent(typeof(CanvasGroup), typeof(Button))]
    internal sealed class DeleteLastEventButtonActivator : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private Button _button;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool Interactable
        {
            get => Button.interactable;
            set => Button.interactable = value;
        }

        private Button Button => _button == null ? _button = GetComponent<Button>() : _button;
        private CanvasGroup CanvasGroup => _canvasGroup == null ? _canvasGroup = GetComponent<CanvasGroup>() : _canvasGroup;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

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
        }

        public void ShowImmediately()
        {
            gameObject.SetActive(true);
            CanvasGroup.DOKill();
            CanvasGroup.alpha = 1f;
        }
        
        public void Hide(Action onComplete = null)
        {
            HideImmediately();
            
            onComplete?.Invoke();
        }

        public void HideImmediately()
        {
            gameObject.SetActive(false);
            CanvasGroup.DOKill();
            CanvasGroup.alpha = 0f;
        }

        public void AddListener(UnityAction call)
        {
            Button.onClick.AddListener(call);
        }

        public void RemoveListener(UnityAction call)
        {
            Button.onClick.RemoveListener(call);
        }
    }
}