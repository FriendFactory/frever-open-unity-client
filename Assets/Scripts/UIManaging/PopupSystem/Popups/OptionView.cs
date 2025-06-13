using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class OptionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _button;

        private Action _onClick;
        
        private void Awake()
        {
            _button.onClick.AddListener(() =>
            {
                _onClick?.Invoke();
            });
        }

        public void Init(string optionName, Action onClicked, Color textColor)
        {
            _text.text = optionName;
            _text.color = textColor;
            _onClick = onClicked;
        }
    }
}