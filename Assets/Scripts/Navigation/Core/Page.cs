using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Navigation.Core
{
    public abstract class Page : MonoBehaviour
    {
        [NonSerialized] protected PageManager Manager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public abstract PageId Id { get; }

        protected bool IsDestroyed { get; private set; }
        protected bool IsDisplayed { get; private set; }
        protected bool IsInitialized { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(PageManager pageManager)
        {
            Manager = pageManager;
            IsInitialized = true;
            OnInit(pageManager);
        }

        public void Display(PageArgs pageArgs, Action onDisplayed, Action onLoadingCanceled)
        {
            if (CheckArgs(pageArgs))
            {
                OnDisplayStart(pageArgs, onDisplayed, onLoadingCanceled);
                IsDisplayed = true;
            }
        }

        public void Hide(Action onHidden = null)
        {
            OnHidingBegin(onHidden);
            IsDisplayed = false;
        }
        
        //---------------------------------------------------------------------
        // Internal 
        //---------------------------------------------------------------------

        internal void CleanUp()
        {
            OnCleanUp();
            IsInitialized = false;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void OnDestroy()
        {
            IsDestroyed = true;
            
            if (!IsInitialized) return;
            
            CleanUp();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract bool CheckArgs(PageArgs pageArgs);
        protected abstract void OnInit(PageManager pageManager);
        protected abstract void OnDisplayStart(PageArgs pageArgs, Action onDisplayed, Action onLoadingCanceled);
        protected abstract void OnHidingBegin(Action onComplete);
        protected virtual void OnCleanUp() {}

        internal abstract PageArgs GetBackToPageArgs();
    }
}
