using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Filtering.UI
{
    public class FilteringButton : MonoBehaviour
    {
        [SerializeField]
        private GameObject _activeStateObject;
        [SerializeField]
        private GameObject _inactiveStateObject;
        [SerializeField]
        private Button _filteringButton;

        private void Awake()
        {
            if (_filteringButton == null)
            {
                _filteringButton = GetComponentInChildren<Button>();
            }
        }

        public void AddButtonAction(UnityAction action)
        {
            _filteringButton.onClick.AddListener(action);
        }

        public void RemoveButtonListener(UnityAction action)
        {
            _filteringButton.onClick.RemoveListener(action);
        }

        public void SetActive(bool isActive)
        {
            _inactiveStateObject.SetActive(!isActive);
            _activeStateObject.SetActive(isActive);
        }
    }
}