using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    public abstract class BaseAnimatedInformationPopup<TConfiguration>: BaseAnimatedPopup<TConfiguration> where TConfiguration : InformationPopupConfiguration
    {
        [Header("Info")]
        [SerializeField] protected TextMeshProUGUI _descriptionText;
        
        protected override void OnConfigure(TConfiguration configuration)
        {
            if (_descriptionText != null && configuration.Description != null)
            {
                _descriptionText.text = configuration.Description;
            }
        }
    }
}