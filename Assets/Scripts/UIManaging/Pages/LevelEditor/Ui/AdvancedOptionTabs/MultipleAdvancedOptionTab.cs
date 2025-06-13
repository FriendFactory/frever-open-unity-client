using System;
using UIManaging.Pages.LevelEditor.Ui.SelectionItems;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs
{
    internal abstract class MultipleAdvancedOptionTab : AdvancedOptionTab
    {
        [SerializeField] private SelectableTabItem TabItem;

        private Toggle _toggle;
        private Action<AdvancedOptionTab> _onToggle;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected abstract string Name { get; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void SetupTab(Action<AdvancedOptionTab> onToggle)
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnToggle);
            _onToggle = onToggle;
            TabItem.SetNameText(Name);
            base.SetupTab(onToggle);
        }

        public void SetToggle(bool value)
        {
            _toggle.isOn = value;
        }

        public override void CleanUp()
        {
            _toggle.onValueChanged.RemoveAllListeners();
            base.CleanUp();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnToggle(bool value)
        {
            DisplayView(value);
            TabItem.SetSelected(value);
            _toggle.interactable = !value;

            if (value)
            {
                _onToggle?.Invoke(this);
            }
        }
    }
}
