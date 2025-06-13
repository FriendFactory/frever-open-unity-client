using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIManaging.Pages.StyleSelection.UI
{
    public class SelfieSelectionElement : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _backgroundButton;

        public void Initialize(UnityAction onClick, UnityAction onBackgroundClick)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(onClick);
            
            
            _backgroundButton.onClick.RemoveAllListeners();
            _backgroundButton.onClick.AddListener(onBackgroundClick);
        }
    }
}