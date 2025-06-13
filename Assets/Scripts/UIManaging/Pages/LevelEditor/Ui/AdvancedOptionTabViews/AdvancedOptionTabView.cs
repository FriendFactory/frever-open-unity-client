using System;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews
{
    public abstract class AdvancedOptionTabView : MonoBehaviour
    {
        public event Action SettingChanged;
        public abstract void Setup();
        public virtual void Display()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public abstract void Reset();
        public abstract void CleanUp();
        public abstract void Discard();
        public abstract void SaveSettings();
        public virtual void UpdateViewComponents() {}

        protected void OnSettingChanged()
        {
            SettingChanged?.Invoke();
        }
    }
}
