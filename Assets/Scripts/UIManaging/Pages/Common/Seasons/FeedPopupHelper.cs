using System;
using System.Collections.Generic;
using Common;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Modules.QuestManaging;
using Modules.UniverseManaging;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Pages.Common.Seasons
{
    public class FeedPopupHelper
    {
        private readonly PopupManager _popupManager;
        private readonly IDataFetcher _dataFetcher;
        private readonly IQuestManager _questManager;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly IUniverseManager _universeManager;

        private readonly HashSet<PopupType> _blockingPopups = new()
        {
            PopupType.NotificationsPermissionLarge,
            PopupType.SeasonPopup
        };

        private Action _onPopupClosedAction;

        public FeedPopupHelper(PopupManager popupManager, IDataFetcher dataFetcher, 
            IQuestManager questManager, AmplitudeManager amplitudeManager, IUniverseManager universeManager)
        {
            _popupManager = popupManager;
            _dataFetcher = dataFetcher;
            _questManager = questManager;
            _amplitudeManager = amplitudeManager;
            _universeManager = universeManager;
        }

        public bool TryShowSeasonStartPopup(Action onPopupClosed = null)
        {
            if (_amplitudeManager.IsOnboardingQuestsFeatureEnabled() && _questManager.CurrentQuestGroupNumber < Constants.Quests.SEASON_QUEST_GROUP_NUMBER)
            {
                return false;
            }

            var currenSeasonId = _dataFetcher.CurrentSeason?.Id.ToString();
            var lastShownSeasonKey = PlayerPrefs.GetString(Constants.Onboarding.SEEN_SEASON_POPUP_IDENTIFIER, null);

            if (currenSeasonId == null || lastShownSeasonKey == currenSeasonId)
            {
                return false;
            }

            PlayerPrefs.SetString(Constants.Onboarding.SEEN_SEASON_POPUP_IDENTIFIER, _dataFetcher.CurrentSeason?.Id.ToString());
            var popupConfiguration = new SeasonPopupConfiguration();
            _popupManager.PushPopupToQueue(popupConfiguration);

            if (onPopupClosed is not null)
            {
                _popupManager.PopupShown += OnPopupShown;
                _onPopupClosedAction = onPopupClosed;
            }

            return true;
        }

        public bool TryShowEndedSeasonPopup()
        {
            return _universeManager.TryShowEndSeasonPopup();
        }

        public bool IsPopupDisplayed(Action onPopupClosed = null)
        {
            if (_popupManager.IsAnyPopupOpen())
            {
                _onPopupClosedAction = onPopupClosed;
                _popupManager.PopupHidden += OnPopupHidden;
                return true;
            }

            return false;
        }
        
        private void OnPopupShown(PopupType type)
        {
            if (!_blockingPopups.Contains(type))
            {
                return;
            }
            _popupManager.PopupShown -= OnPopupShown;
            _popupManager.PopupHidden += OnPopupHidden;
        }

        private void OnPopupHidden(PopupType type)
        {
            if (!_blockingPopups.Contains(type))
            {
                return;
            }
            _popupManager.PopupHidden -= OnPopupHidden;
            _onPopupClosedAction?.Invoke();
            _onPopupClosedAction = null;
        }
    }
}