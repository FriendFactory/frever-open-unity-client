using System;
using Extensions;

namespace UIManaging.Pages.Common.TabsManager
{
    public class TabsManagerArgs
    {
        public event Action OnSelectedTabIndexChangedEvent;

        public TabModel[] Tabs { get; }
        public int SelectedTabIndex { get; private set; }
        public TabModel SelectedTab => Tabs[SelectedTabIndex];

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public TabsManagerArgs(TabModel[] tabs)
        {
            Tabs = tabs;
        }
        
        public TabsManagerArgs(TabModel[] tabs, int initialTabIndex)
        {
            Tabs = tabs;
            SelectedTabIndex = initialTabIndex;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetSelectedTabIndexSilent(int tabIndex)
        {
            SelectedTabIndex = tabIndex;
        }
        
        public void SetSelectedTabIndex(int tabIndex)
        {
            SelectedTabIndex = tabIndex;
            OnSelectedTabIndexChangedEvent?.Invoke();
        }

        public override string ToString()
        {
            return $"{Tabs.ToStringWithSeparator()}";
        }
    }
}