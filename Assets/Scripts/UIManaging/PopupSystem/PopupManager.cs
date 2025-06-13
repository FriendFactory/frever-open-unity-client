using System.Collections.Generic;
using UnityEngine;
using UIManaging.PopupSystem.Popups;
using UIManaging.PopupSystem.Configurations;
using System;
using System.Linq;
using Common;
using Extensions;
using UnityEngine.UI;

namespace UIManaging.PopupSystem
{
    public sealed class PopupManager : MonoBehaviour
    {
        [SerializeField] PopupFactory Factory;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private int _poolLimit = 3;
        [SerializeField] private List<BasePopup> _popupsPool = new List<BasePopup>();

        private readonly List<BasePopup> _openedPopups = new List<BasePopup>();
        private readonly List<RectTransform> _popupParentsByType = new List<RectTransform>();

        private readonly Queue<(PopupConfiguration, RectTransform)> _popupQueue = new Queue<(PopupConfiguration, RectTransform)>();
        
        public event Action<PopupType> PopupShown;
        public event Action<PopupType> PopupHidden;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetupPopup(PopupConfiguration popupConfiguration, RectTransform popupParent = null)
        {
            var popup = GetConfiguredPopupFromPool(popupConfiguration, popupParent);
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = Constants.POPUPS_SORTING_LAYER;
        }
        
        public void PushPopupToQueue(PopupConfiguration popupConfiguration, RectTransform popupParent = null)
        {
            if (_popupQueue.Count == 0 && _openedPopups.All(p => p.NonBlockingQueue))
            {
                SetupPopup(popupConfiguration, popupParent);
                ShowPopup(popupConfiguration.PopupType);
                return;
            }
            
            _popupQueue.Enqueue((popupConfiguration, popupParent));
        }

        public void ShowPopup(PopupType popupType, bool onTop = false)
        {
            var popup = GetPopupFromPool(popupType);
            if (popup == null)
            {
                throw new Exception("Popup should be setuped before show");
            }

            if (onTop)
            {
                popup.transform.parent.SetAsLastSibling();
            }

            popup.Hidden += OnPopupHidden;
            popup.Show();
            
            _openedPopups.Add(popup);
            
            PopupShown?.Invoke(popupType);
        }

        public void CloseAllPopups()
        {
            var temp = new List<BasePopup>(_openedPopups);
            foreach (var popup in temp)
            {
                popup.Hide(null);
            }
        }

        public void ClosePopupByType(PopupType popupType)
        {
            var popup = _openedPopups.Find((p) => p.Type == popupType);
            if (popup != null)
            {
                popup.Hide(null);
            }
        }

        public bool IsAnyPopupOpen()
        {
            return _openedPopups.Any();
        }

        public bool IsPopupOpen(PopupType popupType)
        {
            return _openedPopups.Any(popup => popup.Type == popupType);
        }

        public void RemoveAllFromPool(PopupType popupType)
        {
            _popupsPool.RemoveAll(popup => popup.Type == popupType);
        }
        
        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------

        private void OnPopupHidden(BasePopup popup)
        {
            popup.Hidden -= OnPopupHidden;
            _openedPopups.Remove(popup);
            
            PopupHidden?.Invoke(popup.Type);
            
            if (!_popupsPool.Contains(popup)) Destroy(popup.gameObject);

            if (_popupQueue.Count > 0 && _openedPopups.All(p => p.NonBlockingQueue))
            {
                var nextPopup = _popupQueue.Dequeue();
                SetupPopup(nextPopup.Item1, nextPopup.Item2);
                ShowPopup(nextPopup.Item1.PopupType);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private BasePopup PreparePopup(PopupConfiguration popupConfiguration, RectTransform popupParent = null, bool onTop = false)
        {
            var parent = popupParent;
            if (parent == null)
            {
                parent = _popupParentsByType.Find(p => p.gameObject.name == popupConfiguration.PopupType.ToString());
                if (parent == null)
                {
                    var parentGo = new GameObject(popupConfiguration.PopupType.ToString(), typeof(RectTransform));
                    parentGo.transform.SetParent(transform, false);

                    parent = parentGo.transform as RectTransform;
                    parent.pivot = new Vector2(0.5f, 0.5f);
                    parent.anchorMin = Vector2.zero;
                    parent.anchorMax = Vector2.one;
                    parent.sizeDelta = Vector2.zero;

                    _popupParentsByType.Add(parent);
                }
            }
            
            var popup = Factory.CreatePopup(popupConfiguration, parent);

            if (popup.SortOrder > 0)
            {
                var canvas = parent.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = parent.gameObject.AddComponent<Canvas>();
                }

                canvas.overrideSorting = true;
                canvas.sortingOrder = Constants.POPUPS_SORTING_LAYER + popup.SortOrder;
                canvas.gameObject.AddComponentIfMissed<GraphicRaycaster>();
            }

            popup.SilentHide();

            if (_popupsPool.Count(p => p.Type == popupConfiguration.PopupType) < _poolLimit)
            {
                _popupsPool.Add(popup);
            }
            
            return popup;
        }

        private BasePopup GetPopupFromPool(PopupType popupType)
        {
            return _popupsPool.Find((popup) => popup.Type == popupType && !popup.gameObject.activeSelf);
        }

        private BasePopup GetConfiguredPopupFromPool(PopupConfiguration popupConfiguration, RectTransform popupParent = null)
        {
            var popup = GetPopupFromPool(popupConfiguration.PopupType);
            if (popup == null)
            {
                if (_popupsPool.FindAll(p => p.Type == popupConfiguration.PopupType).Count < _poolLimit)
                {
                    popup = PreparePopup(popupConfiguration, popupParent);
                }
                else
                {
                    popup = _popupsPool.FindLast(p => p.Type == popupConfiguration.PopupType);
                    popup.gameObject.SetActive(false);
                }
            }
            popup.Configure(popupConfiguration);
            return popup;
        }
    }
}