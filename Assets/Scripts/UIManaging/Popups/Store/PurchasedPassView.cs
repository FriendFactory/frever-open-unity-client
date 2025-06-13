using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Popups.Store
{
    public sealed class PurchasedPassView: MonoBehaviour
    {
        [SerializeField] private Button _button;
        private Action _onClicked;

        private void Awake()
        {
            _button.onClick.AddListener(()=>_onClicked?.Invoke());
        }

        public void Init(Action onClicked)
        {
            _onClicked = onClicked;
        }
    }
}