using System;
using Abstract;
using DigitalRubyShared;
using Modules.CameraSystem.CameraSystemCore;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal abstract class AssetSelectionViewManager : BaseContextDataView<AssetSelectorsHolder>
    {
        [FormerlySerializedAs("_assetSelectionManagerView")] [SerializeField] private AssetSelectorView assetSelectorView;
        [SerializeField] private TabsManagerView _tabsManagerView;

        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private FingersScript _fingersScript;

        protected BaseEditorPageModel EditorPageModel;
        private IOutsideViewTapTracker _outsideViewTapTracker;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsOpened => gameObject.activeInHierarchy;
        protected AssetSelectorView AssetSelectorView => assetSelectorView;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action Closed;
        public event Action Opened;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _outsideViewTapTracker = new OutsideViewTapTracker(_fingersScript);
        }

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
            model.SetTabToSelection(true);
            //assetSelectorView.SetSelectedScrollPositions(model);
        }
        
        public void OnTapOutsideView()
        {
            if (assetSelectorView.IsExpandedOrExpanding())
            {
                assetSelectorView.PlayShrinkAnimation();
                return;
            }

            _cameraSystem.StopAnimation();
            assetSelectorView.HideAdvancedAssetSettingsView();
            Close();
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
        }
        
        protected virtual void Close()
        {
            CleanUp();
            assetSelectorView.CleanUp();
            Closed?.Invoke();
            gameObject.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnTabSelectionCompleted(int tabIndex)
        {
            ContextData.OnTabSelected(assetSelectorView, tabIndex);
        }

        private void SetupTapGesture()
        {
            _outsideViewTapTracker.TappedOutsideView += OnTapOutsideView;
            _outsideViewTapTracker.Run();
        }

        private void CleanUpTabGesture()
        {
            _outsideViewTapTracker.TappedOutsideView -= OnTapOutsideView;
            _outsideViewTapTracker.Stop();
            _outsideViewTapTracker.Dispose();
        }
    }
}