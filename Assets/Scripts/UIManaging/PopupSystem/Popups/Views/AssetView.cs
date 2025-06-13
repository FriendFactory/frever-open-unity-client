using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Views
{
    public abstract class AssetView : MonoBehaviour
    {
        public RectTransform ViewArea;
        public abstract void Setup();
        public abstract void Display();
        public abstract void Hide();
        public abstract void Reset();
        public abstract void CleanUp();
    }
}