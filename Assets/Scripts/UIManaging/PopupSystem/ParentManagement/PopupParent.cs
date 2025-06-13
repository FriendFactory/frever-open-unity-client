using JetBrains.Annotations;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    internal class PopupParent: MonoBehaviour, IPopupParent
    {
        [SerializeField] private PopupType _popupType;

        private IPopupParentManager _popupParentManager;
        private RectTransform _target;

        public PopupType PopupType => _popupType;
        public RectTransform Target => _target;

        [Inject, UsedImplicitly]
        private void Construct(IPopupParentManager popupParentManager)
        {
            _popupParentManager = popupParentManager;
            _target = GetComponent<RectTransform>();
            
            _popupParentManager.Register(this);
        }

        private void OnDestroy()
        {
            _popupParentManager.Unregister(this);
        }
    }

    public interface IPopupParent
    {
        PopupType PopupType { get; }
        RectTransform Target { get; }
    }
}