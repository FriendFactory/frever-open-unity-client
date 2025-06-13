using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal sealed class ExpandSelectionViewButton : MonoBehaviour
    {
        [SerializeField] private Sprite _searchIcon;
        [SerializeField] private Sprite _expandIcon;
        [SerializeField] private Image _expandImage;

        private Button _button;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private Button Button => _button == null ? _button = GetComponent<Button>() : _button;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void AddListener(UnityAction onClick)
        {
            Button.onClick.AddListener(onClick);
        }

        public void UseSearchIcon(bool shouldUseSearchIcon)
        {
            _expandImage.sprite = shouldUseSearchIcon ? _searchIcon : _expandIcon;
            _expandImage.SetNativeSize();
        }
        
        public void CleanUp()
        {
            Button.onClick.RemoveAllListeners();
        }
    }
}
