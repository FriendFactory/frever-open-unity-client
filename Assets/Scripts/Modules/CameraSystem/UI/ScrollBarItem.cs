using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.CameraSystem.UI
{
    //todo: remove ui element from CameraSystem assembly
    public class ScrollBarItem : MonoBehaviour
    {
        [SerializeField] private GameObject _selectedGo;
        [SerializeField] private GameObject _unSelectedGo;
        [SerializeField] private TextMeshProUGUI _selectedItemText;
        [SerializeField] private TextMeshProUGUI _unSelectedItemText;
        [SerializeField] private Image _thumbnail;

        private int _id;
        private Toggle _toggle;
        public event Action<int> OnSelected;
        
        public void Setup(int id)
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(Selected);
            _id = id;
            _selectedItemText.text = id.ToString();
            _unSelectedItemText.text = id.ToString();
        }

        public void SetToggle(bool toggle)
        {
            _toggle.isOn = toggle;
        }

        public void SetThumbnail(Sprite thumbnail)
        {
            _thumbnail.sprite = thumbnail;
        }
        private void Selected(bool selected)
        {
            _selectedGo.SetActive(selected);
            _unSelectedGo.SetActive(!selected);
            if (selected)
            {
                OnSelected?.Invoke(_id);
            }
        }
        
    }
}