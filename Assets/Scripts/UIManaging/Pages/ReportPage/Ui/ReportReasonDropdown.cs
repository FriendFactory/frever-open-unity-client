using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Pages.ReportPage.Ui
{
    public class ReportReasonDropdown : MonoBehaviour
    {
        [SerializeField] private GameObject _dropdownOpenedMarker;
        [SerializeField] private GameObject _dropdownClosedMarker;
        [SerializeField] private ExtendedTMP_Dropdown _extendedTmpDropdown;
        
        [SerializeField] private Color[] _itemColors;
        
        private void Awake()
        {
            RefreshMarkers(false);
        }

        private void OnEnable()
        {
            _extendedTmpDropdown.OnPointerClicked += OnPointerClicked;
            _extendedTmpDropdown.Canceled += OnSelect;
            _extendedTmpDropdown.Selected += OnSelect;
        }

        private void OnDisable()
        {
            _extendedTmpDropdown.OnPointerClicked -= OnPointerClicked;
            _extendedTmpDropdown.Canceled -= OnSelect;
            _extendedTmpDropdown.Selected -= OnSelect;
        }

        public void UpdateItemColors()
        {
            var colorIndex = 0;

            foreach (var item in GetComponentsInChildren<ColoredDropdownItem>())
            {
                item.Image.color = _itemColors[colorIndex++ % _itemColors.Length];
            }
        }

        private void OnSelect(BaseEventData baseEventData)
        {
            RefreshMarkers(false);
            UpdateItemColors();
        }

        private void OnPointerClicked()
        {
            RefreshMarkers(true);
            UpdateItemColors();
        }

        private void RefreshMarkers(bool isOpened)
        {
            _dropdownOpenedMarker.SetActive(isOpened);
            _dropdownClosedMarker.SetActive(!isOpened);
        }
    }
}