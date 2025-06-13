using Abstract;
using UnityEngine;

namespace UIManaging.Common.Skeletons
{
    /// <summary>
    ///    Base class for all UI skeletons.
    /// </summary>
    internal sealed class UISkeleton: MonoBehaviour, IDisplayable
    {
        [SerializeField] private bool _showOnAwake = true;
        
        public bool IsDisplayed => gameObject.activeSelf;
        
        private void Awake()
        {
            if (_showOnAwake)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}