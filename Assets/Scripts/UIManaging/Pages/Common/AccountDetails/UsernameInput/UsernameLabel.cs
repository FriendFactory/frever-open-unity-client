using System;
using Abstract;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.EditUsername
{
    public class UsernameLabel : BaseContextDataButton<string> 
    {
        [SerializeField] private TextMeshProUGUI _labelText;

        public event Action<string> Selected;
        
        protected override void OnInitialized()
        {
            _labelText.text = ContextData;
        }

        protected override void OnUIInteracted()
        {
            Selected?.Invoke(ContextData);
        }
    }
}