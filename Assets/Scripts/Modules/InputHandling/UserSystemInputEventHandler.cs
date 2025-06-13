using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Navigation.Core;
using UnityEngine;
using Zenject;
#if UNITY_ANDROID
using SA.Android.App;
#endif

namespace Modules.InputHandling
{
    [UsedImplicitly]
    public sealed class UserSystemInputEventHandler: ITickable, IBackButtonEventHandler
    {
        private class ClickHandler
        {
            public GameObject gameObject;
            public Action callback;
            public bool persistent;
            
            public ClickHandler(GameObject gameObject, Action callback, bool persistent)
            {
                this.gameObject = gameObject;
                this.callback= callback;
                this.persistent = persistent;
            }
        }
        
        [Inject] private readonly PageManager _pageManager;

        private bool IsActive { get; set; } = true;
        
        private Stack<ClickHandler> _clickHandlers = new Stack<ClickHandler>();

        //---------------------------------------------------------------------
        // ITickable implementation
        //---------------------------------------------------------------------

        public void Tick()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            if (_pageManager.IsChangingPage || !IsActive) return;

            var handler = GetTopActiveHandler();
            if (handler != null)
            {
                if (!handler.persistent)
                {
                    _clickHandlers.Pop();
                }
                
                handler.callback?.Invoke();
            }
            else
            {
        #if UNITY_ANDROID
                AN_MainActivity.Instance.MoveTaskToBack(true);
        #endif
            }
        }
        
        //---------------------------------------------------------------------
        // IBackButtonHandler implementation 
        //---------------------------------------------------------------------

        public void AddButton(GameObject gameObject, Action callback, bool persistent = false)
        {
            var last = GetTopActiveHandler();
            if (last != null)
            {
                if (ReferenceEquals(last.gameObject, gameObject)) return;
            }

            _clickHandlers.Push(new ClickHandler(gameObject, callback, persistent));
        }

        public void RemoveButton(GameObject gameObject)
        {
            if (_clickHandlers.Count <= 0) return;
            
            var last = GetTopActiveHandler();
            if (last != null && ReferenceEquals(last.gameObject, gameObject))
            {
                _clickHandlers.Pop();
            }
        }

        public void ProcessEvents(bool state)
        {
            IsActive = state;
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private ClickHandler GetTopActiveHandler()
        {
            var last = _clickHandlers.Count > 0 ? _clickHandlers.Peek() : null;
            // in some cases correct order is not guaranteed due to unpredictability of enabling/disabling of corresponding GOs,
            // so we need to remove inactive handlers from the stack
            while (!last?.gameObject && _clickHandlers.Count > 0)
            {
                _clickHandlers.Pop();
                last = _clickHandlers.Count > 0 ? _clickHandlers.Peek() : null;
            }

            return last;
        }
    }
}