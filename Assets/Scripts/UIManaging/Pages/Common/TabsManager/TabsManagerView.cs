using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.TabsManager
{
    public class TabsManagerView<TArgs> : MonoBehaviour where TArgs: TabsManagerArgs
    {
        public event Action<int> TabSelectionStarted;
        public event Action<int> TabSelectionCompleted;

        [SerializeField] private ViewSpawner _viewSpawner;
        [SerializeField] private ToggleGroup _toggleGroup;

        private HorizontalLayoutGroup _tabsLayoutGroup;
        private CanvasGroup _canvasGroup;
        
        protected IList<ITabView> TabViews;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public TArgs TabsManagerArgs { get; private set; }

        protected bool IsDestroyed { get; private set; }

        private CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup != null)
                {
                    return _canvasGroup;
                }

                _canvasGroup = gameObject.GetComponent<CanvasGroup>();

                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }

                return _canvasGroup;
            }
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnDestroy()
        {
            IsDestroyed = true;
            if (TabsManagerArgs == null) return;
            TabsManagerArgs.OnSelectedTabIndexChangedEvent -= OnToggleValueChangedByApp;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual void Init(TArgs tabsManagerArgs)
        {
            TabsManagerArgs = tabsManagerArgs;
            TabViews = null;
            SpawnTabs();

            TabsManagerArgs.OnSelectedTabIndexChangedEvent -= OnToggleValueChangedByApp;
            TabsManagerArgs.OnSelectedTabIndexChangedEvent += OnToggleValueChangedByApp;

            OnToggleValueChangedByApp();
            RefreshAllTabsVisuals();

            CanvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            CanvasGroup.alpha = 0f;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected virtual void OnTabsSpawned() {}

        protected virtual void OnToggleSetOn(int index, bool setByUser) {}
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnToggleValueChangedByApp()
        {
            _toggleGroup.allowSwitchOff = true;

            foreach (var tab in TabViews)
            {
                var isActive = tab.Index == TabsManagerArgs.SelectedTabIndex;

                if (isActive != tab.Toggle.isOn)
                {
                    tab.SetValueSilent(isActive);
                }

                tab.RefreshVisuals();
            }

            _toggleGroup.allowSwitchOff = false;
            OnToggleSetOn(TabsManagerArgs.SelectedTabIndex, false);
        }

        protected virtual void SpawnTabs()
        {
            var tabViews = _viewSpawner.Spawn<TabModel, TabView>(TabsManagerArgs.Tabs.Where(tab => tab.Enabled)).ToList<ITabView>();

            if (TabViews != null)
            {
                tabViews.InsertRange(0, TabViews);
            }

            TabViews = tabViews;

            foreach (var tab in TabViews)
            {
                tab.OnToggleValueChangedEvent -= OnToggleValueChangedByUser;
                tab.OnToggleValueChangedEvent += OnToggleValueChangedByUser;
                tab.Toggle.group = _toggleGroup;
                _toggleGroup.RegisterToggle(tab.Toggle);
            }

            OnTabsSpawned();
        }

        private void OnToggleValueChangedByUser(int index, bool value)
        {
            if (value)
            {
                TabSelectionStarted?.Invoke(index);
                TabsManagerArgs.SetSelectedTabIndexSilent(index);
                OnToggleSetOn(index, true);
                TabSelectionCompleted?.Invoke(index);
            }
        }

        private void RefreshAllTabsVisuals()
        {
            foreach (var tab in TabViews)
            {
                tab.RefreshVisuals();
            }
        }
    }

    public class TabsManagerView : TabsManagerView<TabsManagerArgs> { }
}