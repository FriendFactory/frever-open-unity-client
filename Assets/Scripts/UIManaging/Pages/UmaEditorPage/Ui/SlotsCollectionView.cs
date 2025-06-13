using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class SlotsCollectionView : CollectionView
    {
        [SerializeField]
        private Button _clearButtonPrefab;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void AddElement(string itemName, Sprite thumb, Action onClick)
        {
            var go = Instantiate(_slotPrefab, _layout.transform, false);
            var item = go.GetComponent<SlotView>();
            item.Setup(itemName, thumb, onClick);
        }

        public void AddClearElement(Action onClick)
        {
            var clearButton = Instantiate(_clearButtonPrefab, _layout.transform, false);
            clearButton.onClick.AddListener(() => {
                onClick();
            });
        }
    }
}