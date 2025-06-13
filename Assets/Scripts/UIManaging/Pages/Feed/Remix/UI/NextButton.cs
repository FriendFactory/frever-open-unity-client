using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.VideoStreaming.Remix.UI
{
    internal class NextButton : MonoBehaviour
    {
        [SerializeField] private Button _fancyButton;

        private int _characterCount;

        public bool Interactable
        {
            set => _fancyButton.interactable = value;
        }

        public void Initialize(Action onClickNext, int totalCount)
        {
            _fancyButton.onClick.RemoveAllListeners();
            _fancyButton.onClick.AddListener(onClickNext.Invoke);
            _fancyButton.interactable = false;

            _characterCount = totalCount;
        }

        public void UpdateText(int count)
        {
            CheckIfNextButtonIsValid(count);
        }
        
        private void CheckIfNextButtonIsValid(int count)
        {
            _fancyButton.interactable = count == _characterCount;
        }
    }
}
