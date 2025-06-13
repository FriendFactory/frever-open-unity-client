using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    public abstract class InformationPopup<TConfiguration> : BasePopup<TConfiguration>
        where TConfiguration : InformationPopupConfiguration
    {
        [SerializeField] protected TextMeshProUGUI _descriptionText;

        protected override void OnConfigure(TConfiguration configuration)
        {
            if (_descriptionText != null && configuration.Description != null)
            {
                _descriptionText.text = configuration.Description;
            }
        }
    }

    internal sealed class InformationPopup : InformationPopup<InformationPopupConfiguration>
    {
    }
}