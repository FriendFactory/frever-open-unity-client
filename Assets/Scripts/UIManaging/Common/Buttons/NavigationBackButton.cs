using Modules.InputHandling;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.Buttons
{
    [RequireComponent(typeof(Button))]
    public class NavigationBackButton: MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private bool _persistent;

        [Inject] private IBackButtonEventHandler _backButtonEventHandler;

        private Button Button => _button ? _button : _button = GetComponent<Button>();

        private void OnEnable()
        {
            _backButtonEventHandler?.AddButton(gameObject, () => Button.onClick?.Invoke(), _persistent);
        }
        
        private void OnDisable()
        {
            _backButtonEventHandler?.RemoveButton(gameObject);
        }
    }
}