using Extensions;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    internal abstract class DescriptionPopup<TConfiguration> : TitleBasePopup<TConfiguration>
        where TConfiguration : InformationPopupConfiguration
    {
        [SerializeField] protected TextMeshProUGUI _descriptionText;

        protected override void OnConfigure(TConfiguration configuration)
        {
            base.OnConfigure(configuration);

            if (_descriptionText == null) return;
            
            _descriptionText.SetActive(!string.IsNullOrEmpty(configuration.Description));
            _descriptionText.text = configuration.Description;
        }
    }
}