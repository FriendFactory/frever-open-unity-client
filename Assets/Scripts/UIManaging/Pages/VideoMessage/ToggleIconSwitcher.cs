using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    public sealed class ToggleIconSwitcher : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private GameObject _onIcon;
        [SerializeField] private GameObject _offIcon;

        void Awake()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool isOn)
        {
            _onIcon.SetActive(isOn);
            _offIcon.SetActive(!isOn);
        }
    }
}
