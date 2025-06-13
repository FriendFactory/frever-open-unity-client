using TMPro;
using UIManaging.Pages.UserSelection;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.StartChat
{
    public class ChatUserSelectionWidget : UserSelectionWidget
    {
        private static readonly int SELECTION_PANEL_COUNTER_HASH = Animator.StringToHash("SelectionPanelCounter");

        [SerializeField] private Animator _panelAnimator;
        [SerializeField] private TextMeshProUGUI _userCounter;

        [Inject] private SnackBarHelper _snackBarHelper;

        private int _defaultUserCount = 1;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        public void UpdateDefaultUserCount(int defaultUserCount)
        {
            _defaultUserCount = defaultUserCount;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            ContextData.ItemSelectionChanged += OnItemSelectionChanged;
            ContextData.SelectionLimitReached += OnSelectionLimitReached;
            
            UpdateCounter();
        }

        protected override void BeforeCleanup()
        {
            if (ContextData != null)
            {
                ContextData.ItemSelectionChanged -= OnItemSelectionChanged;
                ContextData.SelectionLimitReached -= OnSelectionLimitReached;
            }
            
            base.BeforeCleanup();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnItemSelectionChanged(UserSelectionItemModel item)
        {
            UpdateCounter();
            _panelAnimator.SetInteger(SELECTION_PANEL_COUNTER_HASH, ContextData.SelectedItems.Count);
        }

        private void OnSelectionLimitReached()
        {
            _snackBarHelper.ShowFailSnackBar($"Max {ContextData.MaxSelected + _defaultUserCount} members in group");
        }
        
        private void UpdateCounter()
        {
            _userCounter.text = $"{ContextData.SelectedItems.Count + _defaultUserCount}/{ContextData.MaxSelected + _defaultUserCount}";
        }
    }
}