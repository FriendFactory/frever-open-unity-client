using System;
using System.Collections.Generic;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public sealed class DarkActionPopup : BasePopup<DarkActionPopupConfiguration>
    {
        [SerializeField] private RectTransform _buttonsParent;
        [SerializeField] private Button _buttonPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backgroundButton;
        
        private readonly List<GameObject> _actionGameObjects = new List<GameObject>();

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _closeButton.onClick.AddListener(Hide);
            _backgroundButton.onClick.AddListener(Hide);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(Hide);
            _backgroundButton.onClick.RemoveListener(Hide);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Hide(object result)
        {
            base.Hide(result);
            Clear();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(DarkActionPopupConfiguration configuration)
        {
            CreateActionItems(configuration.Variants);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void CreateActionItems(List<KeyValuePair<string, Action>> actionItems)
        {
            foreach (var actionItem in actionItems)
            {
                var button = CreateButton(actionItem.Value);
                var text = button.GetComponentInChildren<TextMeshProUGUI>();
                text.text = actionItem.Key;
            }
        }
        
        private Button CreateButton(Action action)
        {
            var button = Instantiate(_buttonPrefab, _buttonsParent);
            var buttonGameObject = button.gameObject;
            buttonGameObject.SetActive(true);
            _actionGameObjects.Add(buttonGameObject);
                
            button.onClick.AddListener(() =>
            {
                action?.Invoke();
                Hide();
            });

            return button;
        }

        private void Clear()
        {
            foreach (var button in _actionGameObjects)
            {
                Destroy(button.gameObject);
            }

            _actionGameObjects.Clear();
        }
    }
}
