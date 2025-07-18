﻿using System;
using Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Tasks
{
    public class TaskViewAllItem : BaseContextDataView<Action>
    {
        [SerializeField] private Button _viewAllButton;
        
        protected override void OnInitialized()
        {
            _viewAllButton.onClick.AddListener(OnClick);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _viewAllButton.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            ContextData?.Invoke();
        }
    }
}