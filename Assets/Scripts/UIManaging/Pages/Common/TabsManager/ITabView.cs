using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.TabsManager
{
    public interface ITabView
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        Toggle Toggle { get; }
        RectTransform RectTransform { get; }
        int Index { get; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        event Action<int, bool> OnToggleValueChangedEvent;
        event Action<int, string> OnSelected;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        void SetValueSilent(bool value);
        void RefreshVisuals();
    }
}