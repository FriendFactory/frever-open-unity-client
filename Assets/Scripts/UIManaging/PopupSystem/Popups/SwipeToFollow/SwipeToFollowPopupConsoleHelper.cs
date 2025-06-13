using System;
using System.Linq;
using Bridge;
using QFSW.QC;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow
{
    internal sealed class SwipeToFollowPopupConsoleHelper: MonoBehaviour
    {
        [SerializeField] private int _usersCount = 5;
        
        [Inject] private PopupManager _popupManager;
        [Inject] private IBridge _bridge;

        [Command("show_swipe_to_follow_popup", "Shows swipe to follow popup")]
        public void ShowPopup()
        {
            ShowPopup(_usersCount);
        }

        [Command("show_swipe_to_follow_popup", "Shows swipe to follow popup")]
        public async void ShowPopup(int usersCount)
        {
            try
            {
                var usersResult = await _bridge.GetProfiles(usersCount, 0);

                if (!usersResult.IsSuccess)
                {
                    Debug.LogError($"Failed to load users. [Reason]: {usersResult.ErrorMessage}");
                    return;
                }
                
                var popupConfig = new SwipeToFollowPopupConfiguration(usersResult.Profiles.ToList());
                
                _popupManager.SetupPopup(popupConfig);
                _popupManager.ShowPopup(popupConfig.PopupType);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}