using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.OnBoardingPage.UI
{
    public class UsernameLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _targetText;
        [SerializeField] private Button _button;

        public void SetUsername(string randomName, Action onClick = null)
        {
            _targetText.text = randomName;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}