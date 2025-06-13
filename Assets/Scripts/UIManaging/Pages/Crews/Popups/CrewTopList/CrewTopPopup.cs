using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Extensions;
using Extensions.DateTime;
using Modules.Crew;
using UIManaging.Animated.Behaviours;
using UIManaging.EnhancedScrollerComponents;
using UIManaging.Pages.Crews.Sidebar;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;
using UIManaging.Localization;

namespace UIManaging.Pages.Crews.Popups
{
    internal sealed class CrewTopPopup : BasePopup<CrewTopPopupConfiguration>
    {
        [SerializeField] private CrewTopList _crewTopList;
        [SerializeField] private CrewTopListItem _currentCrewBottomItem;
        [SerializeField] private TMP_Text _footerText;
        [SerializeField] private TMP_Dropdown _weekDropdown;
        
        [Space] 
        [SerializeField] private List<Button> _closeButtons = new List<Button>();
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedBehaviour;
        
        [Inject] private IBridge _bridge;
        [Inject] private CrewService _crewService;
        [Inject] private CrewPageLocalization _localization;

        private void Awake()
        {
            _weekDropdown.AddOptions(new List<TMPro.TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData(_localization.TopThisWeekOption),
                new TMP_Dropdown.OptionData(_localization.TopLastWeekOption)
            });
            
            _weekDropdown.onValueChanged.AddListener(OnWeekDropdownValueChanged);
        }

        private void OnWeekDropdownValueChanged(int index)
        {
            switch (index)
            {
                case 0:
                    InitList(null);
                    break;
                case 1:
                    InitList(Configs.CompetitionTime.AddDays(-7).ToString("yyyy-MM-dd"));
                    break;
            }
        }

        private void OnEnable()
        {
            if (Configs is null) return;

            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClicked));
            _animatedBehaviour.PlayInAnimation(null);
        }

        private void OnDisable()
        {
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
        }

        protected override void OnConfigure(CrewTopPopupConfiguration configuration)
        {
            _weekDropdown.SetValueWithoutNotify(0);
            InitList(null);
        }

        private async void InitList(string date)
        {
            var result = await _bridge.GetCrewsTopList(string.Empty, 50, 0, date, null, default);

            if (!result.IsSuccess)
            {
                Debug.LogError("Failed to load crew top list");
                return;
            }
            
            var currentCrewModel = new CrewTopInfo()
            {
                Id = _crewService.Model.Id,
                Name = _crewService.Model.Name,
                TrophyScore = _crewService.Model.Competition.TrophyScore,
                Files = _crewService.Model.Files
            };

            var currentCrewPlace = result.Models.ToList().FindIndex(item => item.Id == _crewService.Model.Id) + 1;
            
            _currentCrewBottomItem.Initialize(new CrewTopListItemModel(currentCrewModel, currentCrewPlace));

            var place = 1;
            _crewTopList.Initialize(new BaseEnhancedScroller<CrewTopListItemModel>(
                                        result.Models.Select(crew => new CrewTopListItemModel(crew, place++)).ToList()));

            _footerText.text = date == null
                ? string.Format(_localization.TrophyHuntTimeLeftFormat, Configs.CompetitionTime.GetFormattedTimeLeft())
                : string.Format(_localization.LastUpdatedTimeFormat, date);
        }

        private void OnCloseButtonClicked()
        {
            _animatedBehaviour.PlayOutAnimation(OnOutAnimationCompleted);

            void OnOutAnimationCompleted()
            {
                Hide();
            }
        }
    }
}