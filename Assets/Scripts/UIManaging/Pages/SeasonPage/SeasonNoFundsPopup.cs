using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.SeasonPage
{
    public class SeasonNoFundsPopup : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveAllListeners();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

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