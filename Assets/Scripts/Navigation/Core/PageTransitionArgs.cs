using System;

namespace Navigation.Core
{
    public struct PageTransitionArgs
    {
        public bool SaveCurrentPageToHistory;
        public bool HidePreviousPageOnOpen;
        public bool LeavePreviousPageOpenOnTransitionFinished;
        public PageArgs CurrentPageArgs;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public Action TransitionStartedCallback;
        public Action TransitionFinishedCallback;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool OverrideCurrentPageArgs => CurrentPageArgs != null;

        //---------------------------------------------------------------------
        // Static
        //---------------------------------------------------------------------

        internal static PageTransitionArgs Default()
        {
            return new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
            };
        }
    }
}