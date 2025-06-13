using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    public abstract class AssetSettingsView : MonoBehaviour
    {
        public abstract void Setup();

        public void Display()
        {
            PartialDisplay();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            PartialHide();
            gameObject.SetActive(false);
        }

        public abstract void PartialHide();
        public abstract void PartialDisplay();

        public virtual void CleanUp() {}
    }
}
