using System;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs
{
    internal abstract class AdvancedOptionTab : MonoBehaviour
    {
        [SerializeField] protected AdvancedOptionTabView AdvancedOptionTabView;

        public virtual void SetupTab(Action<AdvancedOptionTab> onToggle)
        {
            AdvancedOptionTabView.Setup();
        }

        public void DisplayView(bool show)
        {
            if (show)
            {
                AdvancedOptionTabView.Display();
            }
            else
            {
                AdvancedOptionTabView.Hide();
            }
        }

        public virtual void CleanUp()
        {
            AdvancedOptionTabView.CleanUp();
        }

        public void Reset()
        {
            AdvancedOptionTabView.Reset();
        }
        
        public void UpdateViewComponents()
        {
            AdvancedOptionTabView.UpdateViewComponents();
        }

        public void SaveSettingsBeforeEditing()
        {
            AdvancedOptionTabView.SaveSettings();
        }

        public void DiscardChanges()
        {
            AdvancedOptionTabView.Discard();
        }
    }
}
