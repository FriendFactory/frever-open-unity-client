using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.DiscoveryPage
{
    internal abstract class BaseDiscoverySection : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] protected RectTransform _header;
        [SerializeField] private Button _moreButton;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _moreButton.onClick.AddListener(OnMoreButtonClicked);
        }

        protected virtual void OnDestroy()
        {
            _moreButton.onClick.RemoveListener(OnMoreButtonClicked);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract void OnMoreButtonClicked();
    }
}