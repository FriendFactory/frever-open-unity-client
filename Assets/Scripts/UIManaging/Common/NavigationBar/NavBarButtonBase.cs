using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common
{
    [RequireComponent(typeof(Button))]
    public abstract class NavBarButtonBase : MonoBehaviour
    {
        [SerializeField] private Button _button;

        [Inject] protected PageManager PageManager;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnButtonClicked);
        }

        protected abstract void OnButtonClicked();
    }
}