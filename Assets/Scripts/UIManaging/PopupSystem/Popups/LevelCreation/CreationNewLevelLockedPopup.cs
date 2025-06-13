using Common;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.LevelCreation
{
    public sealed class CreationNewLevelLockedPopupConfig : PopupConfiguration
    {
        public int ChallengesCountToUnlockNewLevelCreation;
        
        public CreationNewLevelLockedPopupConfig(int challengesCountToUnlockNewLevelCreation) : base(PopupType.CreationNewLevelLockedPopup, null)
        {
            ChallengesCountToUnlockNewLevelCreation = challengesCountToUnlockNewLevelCreation;
        }
    }
    
    internal sealed class CreationNewLevelLockedPopup: BasePopup<CreationNewLevelLockedPopupConfig>
    {
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private Button _exploreChallengesButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private ImageByKeyLoader _backgroundImageLoader;
        [Inject] private PageManager _pageManager;

        private void Awake()
        {
            _exploreChallengesButton.onClick.AddListener(() =>
            {
                OpenChallengesPage();
                Hide();
            });
            
            _closeButton.onClick.AddListener(Hide);
        }

        protected override void OnConfigure(CreationNewLevelLockedPopupConfig configuration)
        {
            _description.text =
                $"You need to complete {configuration.ChallengesCountToUnlockNewLevelCreation} challenges\nbefore you can use the video editor";
        }

        public override void Show()
        {
            base.Show();
            _backgroundImageLoader.LoadImageAsync(Constants.FileKeys.LEVEL_CREATION_LOCKED_BG);
        }

        private void OpenChallengesPage()
        {
            _pageManager.MoveNext(new TasksPageArgs());
        }
    }
}