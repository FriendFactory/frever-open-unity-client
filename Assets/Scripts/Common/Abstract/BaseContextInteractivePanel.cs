using System;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Abstract
{
    public class BaseContextSelectablePanel<TModel> : BaseContextPanel<TModel>
    {
        [SerializeField] private Button _button;

        public event Action Selected;
        
        public bool Interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }
        
        protected override void OnInitialized()
        {
            _button.onClick.AddListener(OnSelected);
        }

        protected override void BeforeCleanUp()
        {
            _button.onClick.RemoveListener(OnSelected);
        }

        protected virtual void OnSelected() => Selected?.Invoke();
    }
}