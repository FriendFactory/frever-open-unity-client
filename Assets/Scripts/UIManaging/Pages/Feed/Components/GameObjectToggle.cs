using System;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    [RequireComponent(typeof(Toggle))]
    public class GameObjectToggle : MonoBehaviour
    {
        [SerializeField] private GameObject offGameObject;
        [SerializeField] private GameObject onGameObject;

        private Toggle _toggle;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            OnToggleValueChanged(_toggle.isOn);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool value)
        {
            offGameObject.SetActive(!value);
            onGameObject.SetActive(value);
        }
    }
}

