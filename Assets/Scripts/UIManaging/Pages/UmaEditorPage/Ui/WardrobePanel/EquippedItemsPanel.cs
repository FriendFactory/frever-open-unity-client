using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common;
using Common.UI;
using Extensions;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class EquippedItemsPanel : MonoBehaviour
    {
        public event Action<IEntity> WardrobeItemSelected;
        
        [SerializeField]
        private GameObject[] _objectsToHide;
        [SerializeField]
        private QuickClickDetector _outsideButton;
        [SerializeField]
        private EquippedItemsHolder _equippedItemsHolder;
        [SerializeField]
        private TextMeshProUGUI _noAssetsAvailableText;
        
        private bool[] _previousActiveStates;
        private int _objectsCount = -1;

        private void Awake()
        {
            _outsideButton.Clicked += OnClickOutside;
            _equippedItemsHolder.WardrobeItemSelected += OnItemSelected;
        }

        private void OnDestroy()
        {
            _outsideButton.Clicked -= OnClickOutside;
            _equippedItemsHolder.WardrobeItemSelected -= OnItemSelected;
        }

        public void Setup(IEnumerable<IEntity> entities)
        {
            _objectsCount = entities.Count();
            _equippedItemsHolder.UpdateSelections(entities);
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
            
            _noAssetsAvailableText.SetActive(_objectsCount < 1);
            _equippedItemsHolder.ShowItems();
            
            if (_objectsToHide is null)
            {
                return;
            }
            _previousActiveStates = new bool[_objectsToHide.Length];
            for (int i = 0; i < _objectsToHide.Length; i++)
            {
                _previousActiveStates[i] = _objectsToHide[i].activeSelf;
                _objectsToHide[i].SetActive(false);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);   
            if (_objectsToHide is null || _previousActiveStates is null)
            {
                return;
            }      
            for (int i = 0; i < _objectsToHide.Length; i++)
            {
                _objectsToHide[i].SetActive(_previousActiveStates[i]);
            }
        }

        public void OnClickOutside()
        {
            Hide();
        }

        public void SetGenderID(long id)
        {
            _equippedItemsHolder.GenderId = id;
        }

        private void OnItemSelected(IEntity item)
        {
            WardrobeItemSelected?.Invoke(item);
        }
    }
}
