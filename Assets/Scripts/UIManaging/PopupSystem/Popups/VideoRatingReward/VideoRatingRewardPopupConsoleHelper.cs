using Bridge.Models.VideoServer;
using UnityEngine;
using Zenject;
using QFSW.QC;

namespace UIManaging.PopupSystem.Popups.VideoRatingReward
{
    internal sealed class VideoRatingRewardPopupConsoleHelper: MonoBehaviour
    {
        [Inject] private PopupManager _popupManager;

        [Command("show-reward-popup")]
        public void ShowRewardPopup(int rating)
        {
            var ratingResult = new RatingResult()
            {
                Rating = rating,
                SoftCurrency = 200,
            };
            var popupConfig = new VideoRatingRewardPopupConfiguration(13777, ratingResult);
            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(popupConfig.PopupType);
        }
    }
}