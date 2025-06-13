using System;
using Abstract;
using DigitalRubyShared;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    internal sealed class ProfilePhotoEditorAssetSelectionManager: BaseContextDataView<AssetSelectorsHolder>
    {
        [SerializeField] private AssetSelectorView _assetSelectorView;
        [SerializeField] private TabsManagerView _tabsManagerView;
        
        [Inject] private FingersScript _fingersScript;
        
        private TapGestureRecognizer _tapGesture;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action Closed;
        public event Action Opened;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            SetupTapGesture();
        }
        
        private void OnDisable()
        {
            CleanUpTabGesture();
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetSelectedItemScrollPositions(MainAssetSelectorModel model)
        {
            model.SetTabToSelection();
            //_assetSelectorView.SetSelectedScrollPositions(model);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            OnTabSelectionCompleted(ContextData.TabsManagerArgs.SelectedTabIndex);

            var shouldShowTabs = ContextData.TabsManagerArgs.Tabs.Length > 1;
            _tabsManagerView.gameObject.SetActive(shouldShowTabs);

            if (shouldShowTabs)
            {
                _tabsManagerView.Init(ContextData.TabsManagerArgs);
                _tabsManagerView.TabSelectionCompleted += OnTabSelectionCompleted;
            }
        
            Opened?.Invoke();
        }
        
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _tabsManagerView.TabSelectionCompleted -= OnTabSelectionCompleted;
            _assetSelectorView.CleanUp();
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------
        
        private void OnTabSelectionCompleted(int tabIndex)
        {
            ContextData.OnTabSelected(_assetSelectorView, tabIndex);
        }
        
        private void SetupTapGesture()
        {
            _tapGesture = _tapGesture ?? new TapGestureRecognizer();
            _fingersScript.AddGesture(_tapGesture);
            _tapGesture.StateUpdated += UpdateGesture;
        }

        private void CleanUpTabGesture()
        {
            if (_tapGesture == null) return;
            
            _tapGesture.StateUpdated -= UpdateGesture;
            _tabsManagerView.TabSelectionCompleted -= OnTabSelectionCompleted;
            _fingersScript.RemoveGesture(_tapGesture);
            _tapGesture.Dispose();
        }
        
        private void UpdateGesture(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                OnTapOutsideView();
            }
        }
        
        private void CloseView()
        {
            Close();
        }
        
        private void Close()
        {
            CleanUp();
            Closed?.Invoke();
            
            gameObject.SetActive(false);
        }

        private void OnTapOutsideView()
        {
            if (_assetSelectorView.IsExpandedOrExpanding())
            {
                _assetSelectorView.PlayShrinkAnimation();
                return;
            }
            
            _assetSelectorView.HideAdvancedAssetSettingsView();
            CloseView();
        }
    }
}