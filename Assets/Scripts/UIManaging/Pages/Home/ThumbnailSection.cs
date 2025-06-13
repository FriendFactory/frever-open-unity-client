using System;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.FollowersPage.UI;
using UIManaging.Pages.SeasonPage;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Home
{
    internal sealed class ThumbnailSection : MonoBehaviour
    {
        [SerializeField] private HomepageThumbnail _homepageThumbnail;

        [Space] [SerializeField] private Button _seasonButton;
        [SerializeField] private TMP_Text _seasonName;
        [SerializeField] private TMP_Text _userLevel;
        [SerializeField] private GameObject _rewardIndicator;
        [SerializeField] private TMP_Text _rewardsCount;

        [Space] [SerializeField] private Button _outfitButton;
        [SerializeField] private Button _charactersButton;
        [SerializeField] private Button _friendsButton;
        [SerializeField] private Button _editCharacterButton;
        [SerializeField] private Toggle _ipToggle;

        [Inject] private LocalUserDataHolder _userData;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private PageManager _pageManager;
        [Inject] private CharacterManager _characterManager;
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private IBridge _bridge;
        [Inject] private FollowersManager _followersManager;
        [Inject] private SeasonRewardsHelper _seasonRewardsHelper;

        public event Action<bool> RaceToggled;

        private MetadataStartPack MetadataStartPack => _dataFetcher.MetadataStartPack;
        
        private void OnEnable()
        {
            _outfitButton.onClick.AddListener(OnOutfitButtonClick);
            _charactersButton.onClick.AddListener(OnCharacterButtonClick);
            _friendsButton.onClick.AddListener(OnFriendsButtonClick);
            _editCharacterButton.onClick.AddListener(OnEditCharacterButtonClick);
            _seasonButton.onClick.AddListener(OnSeasonButtonClick);
            
            SetupRaceToggle();
        }
        
        private void OnDisable()
        {
            _outfitButton.onClick.RemoveListener(OnOutfitButtonClick);
            _charactersButton.onClick.RemoveListener(OnCharacterButtonClick);
            _friendsButton.onClick.RemoveListener(OnFriendsButtonClick);
            _editCharacterButton.onClick.RemoveListener(OnEditCharacterButtonClick);
            _seasonButton.onClick.RemoveListener(OnSeasonButtonClick);
            _ipToggle.onValueChanged.RemoveListener(OnRaceToggled);
        }

        public void Initialize()
        {
            _homepageThumbnail.Initialize();

            SetupSeasonButton();
        }

        public void UpdateThumbnail()
        {
            _homepageThumbnail.UpdateImages();
        }

        private void OnEditCharacterButtonClick()
        {
            SetButtonsActive(false);
            OpenCharacterEditor();
        }

        private void OnOutfitButtonClick()
        {
            const long wardrobeCategoryID = 2;
            SetButtonsActive(false);
            OpenCharacterEditor(wardrobeCategoryID);
        }

        private async void OpenCharacterEditor(long? categoryId = null)
        {
            if (_characterManager.SelectedCharacter == null)
            {
                _popupManagerHelper.OpenMainCharacterIsNotSelectedPopup();
                SetButtonsActive(true);
                return;
            }

            var fullCharacterResp = await _characterManager.GetCharacterAsync(_characterManager.SelectedCharacter.Id);
            _scenarioManager.ExecuteCharacterEditing(fullCharacterResp, categoryId);
        }

        private void OnCharacterButtonClick()
        {
            var activeUniverses = _dataFetcher.MetadataStartPack.GetActiveUniverses().ToArray();
            if (activeUniverses.Length == 1)
            {
                var raceId = activeUniverses[0].Races.First().RaceId;
                var race = _dataFetcher.MetadataStartPack.GetRace(raceId);
                OnRaceSelected(race);
                return;
            }
            var pageArgs = new RaceSelectionPageArs
            {
                RaceSelected = OnRaceSelected,
                MoveBackRequested = _pageManager.MoveBack
            };
            _pageManager.MoveNext(pageArgs);
            return;

            void OnRaceSelected(Race race)
            {
                _pageManager.MoveNext(new UmaAvatarArgs
                {
                    TargetGenders = race.Genders.Select(x=>x.Id).ToArray()
                });
            }
        }

        private void OnFriendsButtonClick()
        {
            _pageManager.MoveNext(PageId.FollowersPage,
                                  new UserFollowersPageArgs(_followersManager, 0) { IsLocalUser = true });
        }

        private void SetupSeasonButton()
        {
            if (_dataFetcher.CurrentSeason is null)
            {
                _seasonButton.SetActive(false);
                return;
            }

            _seasonName.text = _dataFetcher.CurrentSeason.Title;
            _userLevel.text = _userData.LevelingProgress.Xp.CurrentLevel.Level.ToString();

            SetupSeasonRewardBadge();
        }

        private void SetupRaceToggle()
        {
            var userHasDiffRacesCharacters = _characterManager.RaceMainCharacters.Count > 1;
            var multipleUniversesActive = MetadataStartPack.GetActiveUniversesCount() > 1;
            var shouldBeActive = userHasDiffRacesCharacters && multipleUniversesActive;
            _ipToggle.gameObject.SetActive(shouldBeActive);
            if (!shouldBeActive) return;
            
            var raceId = _characterManager.RaceMainCharacters
                .First(c => c.Value == _characterManager.SelectedCharacter.Id).Key;
            // (Race ID == 1) represents Frever Race, which is left position of toggle, which is 'false' value
            _ipToggle.isOn = raceId != 1;
            _ipToggle.onValueChanged.AddListener(OnRaceToggled);
        }

        private async void SetupSeasonRewardBadge()
        {
            var availableRewardCount = await _seasonRewardsHelper.IsClaimableRewardAvailable(true);

            if (this.IsDestroyed())
            {
                return;
            }

            _rewardIndicator.SetActive(availableRewardCount != 0);
            _rewardsCount.text = availableRewardCount.ToString();
        }

        private void OnSeasonButtonClick()
        {
            var args = new SeasonPageArgs(SeasonPageArgs.Tab.Rewards);
            _pageManager.MoveNext(PageId.SeasonInfo, args);
        }

        private void OnRaceToggled(bool value)
        {
            RaceToggled?.Invoke(value);
        }

        private void SetButtonsActive(bool isActive)
        {
            _outfitButton.enabled = isActive;
            _charactersButton.enabled = isActive;
            _friendsButton.enabled = isActive;
            _editCharacterButton.enabled = isActive;
            _ipToggle.enabled = isActive;
            _ipToggle.gameObject.SetActive(isActive && _characterManager.RaceMainCharacters.Count > 1);
        }
    }
}