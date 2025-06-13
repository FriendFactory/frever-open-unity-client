using System;
using UIManaging.Common.SelectionPanel;

namespace UIManaging.PopupSystem.Popups.Share
{
    public abstract class ShareSelectionItemModel: IShareSelectionItemModel, ISelectionItemModel
    {
        public abstract long Id { get; }
        public abstract string Title { get; }
        
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
                
                if (_isSelected != value)
                {
                    _isSelected = value;
                    SelectionChanged?.Invoke();
                }
            } 
        }
        
        public bool IsLocked { get; set; }
        
        public event Action SelectionChanged;
        public event Action SelectionChangeLocked;
        
        private bool _isSelected;

        protected ShareSelectionItemModel(bool isSelected)
        {
            _isSelected = isSelected;
        }
    }
}