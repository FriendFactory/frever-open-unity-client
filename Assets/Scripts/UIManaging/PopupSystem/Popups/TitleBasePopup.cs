using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    public abstract class TitleBasePopup<TConfiguration> : ConfigurableBasePopup<TConfiguration> 
        where TConfiguration : PopupConfiguration 
    {
        [SerializeField] private TextMeshProUGUI _titleText;

        protected override void OnConfigure(TConfiguration configuration)
        {
            if (_titleText != null)
            {
                _titleText.text = configuration.Title;
                var showTitle = !string.IsNullOrEmpty(configuration.Title);
                _titleText.gameObject.SetActive(showTitle);
            }
        }
    }
}