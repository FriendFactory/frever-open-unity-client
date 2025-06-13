using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Core
{
    public class TMPDropdownWithDividers : TMP_Dropdown
    {
        [SerializeField] private GameObject _dividerPrefab;

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            InstantiateDividers();
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            InstantiateDividers();
        }

        private void InstantiateDividers()
        {
            var parent = GetComponentInChildren<DropdownItem>().transform.parent;
            var dividerSiblingIndex = 2;
            var dividersCount = options.Count - 1;
            
            for (var i = 0; i < dividersCount; i++)
            {
                var divider = Instantiate(_dividerPrefab, parent);
                divider.SetActive(true);
                divider.transform.SetSiblingIndex(dividerSiblingIndex);
                dividerSiblingIndex += 2;
            }
        }
    }
}