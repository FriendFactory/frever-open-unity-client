using System;
using Bridge.Models.ClientServer.Level.Shuffle;
using UIManaging.Common.SelectionPanel;

namespace UIManaging.PopupSystem.Popups.Shuffle
{
    public class AssetTypeModel: ISelectionItemModel
    {
        public ShuffleAssets Type { get; }
        public long Id => (long) Type;
        public bool IsLocked { get; set; }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (IsLocked)
                {
                    SelectionChangeLocked?.Invoke();
                    SelectionChanged?.Invoke();
                    return;
                }
                
                if (_isSelected == value) return;

                _isSelected = value;
                SelectionChanged?.Invoke();
            }
        }

        public event Action SelectionChanged;
        public event Action SelectionChangeLocked;

        private bool _isSelected;

        public AssetTypeModel(ShuffleAssets type)
        {
            Type = type;
        }
    }
}