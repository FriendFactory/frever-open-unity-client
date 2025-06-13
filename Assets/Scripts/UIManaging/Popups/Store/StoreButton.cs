using System;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Popups.Store
{
    public sealed class StoreButton : MonoBehaviour
    {
        [SerializeField] private bool _showSeasonRewardsButtonOnStore = true;
        
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private LocalUserDataHolder _localUserDataHolder;

        public event Action UserBalanceUpdated;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            _popupManagerHelper.ShowStorePopup(_showSeasonRewardsButtonOnStore, RefreshUserBalance);
        }

        private async void RefreshUserBalance()
        {
            await _localUserDataHolder.RefreshUserInfoAsync();
            UserBalanceUpdated?.Invoke();
        }
    }
}
