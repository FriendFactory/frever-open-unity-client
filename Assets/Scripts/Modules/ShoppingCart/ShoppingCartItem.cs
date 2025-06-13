using Bridge;
using Bridge.Models.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Common.ShoppingCart
{
    public abstract class ShoppingCartItem : MonoBehaviour
    {
        public event Action<IEntity, bool> SelectionChanged;
        public bool IsSelected = true;
        public AssetOfferInfo AssetOffer { get; private set; }
        public long MinRequiredLevel => _levelRequirable.SeasonLevel.HasValue ? _levelRequirable.SeasonLevel.Value : 0;

        protected IEntity Entity;
        private IMinLevelRequirable _levelRequirable;
        private Toggle _toggle;

        private Toggle Toggle
        {
            get
            {
                if (_toggle == null)
                {
                    _toggle = GetComponent<Toggle>();
                }

                return _toggle;
            }
        }

        public virtual void Init(IBridge bridge)
        {
        }

        public virtual void Setup<T>(T entity) where T : IEntity, IThumbnailOwner, IMinLevelRequirable, IPurchasable
        {
            Entity = entity;
            _levelRequirable = entity;
            AssetOffer = entity.AssetOffer;
        }
        

        public void OnToggleChanged(bool isOn)
        {
            IsSelected = isOn;
            SelectionChanged?.Invoke(Entity, isOn);
        }

        public bool IsLocked()
        {
            var levelReq = Entity as IMinLevelRequirable;
            return levelReq.SeasonLevel.HasValue;
        }

        public void SetInteractable(bool interactable)
        {
            Toggle.interactable = interactable;
        }

    }
}
