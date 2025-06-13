using System;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public sealed class SeasonInfoPopupConfiguration: PopupConfiguration
    {
        public Action OnChallengesButtonClickCallback;

        public SeasonInfoPopupConfiguration(): base(PopupType.SeasonInfo, null)
        {
        }
    }
    
    internal sealed class SeasonInfoPopup: BasePopup<SeasonInfoPopupConfiguration>
    {
        [SerializeField] private Button _likesButton;
        [SerializeField] private Button _challengesButton;
        [SerializeField] private Button _overlayButton;

        private void OnEnable()
        {
            _likesButton.onClick.AddListener(Hide);
            _challengesButton.onClick.AddListener(Hide);
            _overlayButton.onClick.AddListener(Hide);
            
            _challengesButton.onClick.AddListener(()=> Configs.OnChallengesButtonClickCallback?.Invoke());
        }

        protected override void OnConfigure(SeasonInfoPopupConfiguration configuration)
        {
        }
    }
}