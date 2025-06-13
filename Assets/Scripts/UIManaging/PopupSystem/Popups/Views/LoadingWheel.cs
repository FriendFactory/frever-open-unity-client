using System;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Views
{
    public class LoadingWheel : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
    
        private float CurrentAlpha
        {
            get => _canvasGroup.alpha;
            set => _canvasGroup.alpha = value;
        }
        private float _targetAlpha;
        private bool _changing;
    
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void Update()
        {
            if(!_changing)
                return;

            if (Math.Abs(CurrentAlpha - _targetAlpha) < 0.0001f)
            {
                _changing = false;
                CurrentAlpha = _targetAlpha;
                gameObject.SetActive(_targetAlpha > 0.01f);
                return;
            }

            CurrentAlpha = Mathf.Lerp(CurrentAlpha, _targetAlpha, 10 * Time.deltaTime);
        }

        public void StartFadeOut()
        {
            gameObject.SetActive(true);
            _targetAlpha = 0;
            _changing = true;
        }

        public void StartFadeIn()
        {
            gameObject.SetActive(true);
            _targetAlpha = 1;
            _changing = true;
        }

        public void Reset()
        {
            CurrentAlpha = 0;
            gameObject.SetActive(false);
        }
    }
}
