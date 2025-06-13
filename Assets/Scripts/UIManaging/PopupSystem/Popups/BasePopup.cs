using System;
using UnityEngine;
using UnityEngine.UI;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.PopupSystem.Popups
{
    public abstract class LegacyTitleBasePopup : BasePopup
    {
        [SerializeField] private Text _titleText;
        public override void Configure(PopupConfiguration configuration)
        {
            base.Configure(configuration);
            if (_titleText != null && configuration.Title != null)
            {
                _titleText.text = configuration.Title;
            }
        }
    }

    public abstract class BasePopup : MonoBehaviour
    {
        [SerializeField] private PopupType _type;
        [SerializeField] private int _sortOrder;
        internal int SortOrder => _sortOrder;
        
        public event Action<object> OnClose;
        public event Action<BasePopup> Hidden;
        public PopupType Type => _type;
        public virtual bool NonBlockingQueue => false;
        public bool IsShown { get; protected set; }

        public virtual void Configure(PopupConfiguration configuration)
        {
            OnClose = configuration.OnClose;
        }
        
        public virtual void Show()
        {
            if (IsShown) return;
            gameObject.SetActive(true);
            IsShown = true;
        }

        public virtual void Hide() => Hide(null);   // UnityEditor Inspector isn't able to handle methods with default parameter(s) (i.g. Button events)

        public virtual void Hide(object result)
        {
            if (!IsShown) return;
            
            OnClose?.Invoke(result);
            OnClose = null;
            SilentHide();
            OnHidden();
            IsShown = false;
            Hidden?.Invoke(this);
        }

        public void SilentHide()
        {
            gameObject.SetActive(false);
        }
        
        protected virtual void OnHidden() { }
    }
    
    public abstract class BasePopup<TConfiguration> : LegacyTitleBasePopup 
        where TConfiguration : PopupConfiguration
    {
        protected TConfiguration Configs;
        
        public sealed override void Configure(PopupConfiguration configuration)
        {
            base.Configure(configuration);
            
            // Validate configuration
            if (configuration is TConfiguration validatedConfiguration)
            {
                Configs = validatedConfiguration;
                OnConfigure(validatedConfiguration);
            }
            else
            {
                Debug.LogError($"Wrong configuration type {configuration.GetType()} for popup {GetType()}!");
            }
        }

        protected abstract void OnConfigure(TConfiguration configuration);
    }
}