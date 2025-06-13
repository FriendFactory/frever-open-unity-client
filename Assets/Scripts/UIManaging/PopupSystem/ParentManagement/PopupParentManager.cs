using System.Collections.Generic;
using JetBrains.Annotations;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    public interface IPopupParentManager
    {
        void Register(IPopupParent popupParent);
        void Unregister(IPopupParent popupParent);
        bool TryGetParent(PopupType popupType, out RectTransform parent);
    }

    [UsedImplicitly]
    public sealed class PopupParentManager : IPopupParentManager
    {
        // TODO: replace with HashSet?
        private readonly Dictionary<PopupType, RectTransform> _transformMap = new();

        public void Register(IPopupParent popupParent)
        {
            _transformMap[popupParent.PopupType] = popupParent.Target;
        }

        public void Unregister(IPopupParent popupParent)
        {
            if (!_transformMap.ContainsKey(popupParent.PopupType)) return;
            
            _transformMap.Remove(popupParent.PopupType);
        }

        public bool TryGetParent(PopupType popupType, out RectTransform parent)
        {
            return _transformMap.TryGetValue(popupType, out parent);
        }
    }
}