using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaAvatarPage.Ui
{
    public class AddCharacterCarouselItem : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private Action _onClick;

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        public void Initialize(Action onClick)
        {
            _onClick = onClick;
        }

        private void OnClick()
        {
            _onClick?.Invoke();
        }
    }
}