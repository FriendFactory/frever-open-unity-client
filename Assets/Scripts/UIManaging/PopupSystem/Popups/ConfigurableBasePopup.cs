using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    public abstract class ConfigurableBasePopup<TConfiguration> : BasePopup 
        where TConfiguration : PopupConfiguration
    {
        protected TConfiguration Config;
        
        public sealed override void Configure(PopupConfiguration configuration)
        {
            base.Configure(configuration);
            
            if (configuration is TConfiguration validatedConfiguration)
            {
                Config = validatedConfiguration;
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