using System.Collections.Generic;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common
{
    public class NavigationBarToggle: MonoBehaviour
    {
        [SerializeField] private List<PageId> _targetPages;

        [Space]
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _buttonOn;
        [SerializeField] private GameObject _buttonOff;

        public IEnumerable<PageId> TargetPages => _targetPages;

        public void Switch(bool isOn)
        {
            if (_button == null) return;

            _buttonOn.SetActive(isOn);
            _buttonOff.SetActive(!isOn);
            _button.interactable = !isOn;
        }
    }
}