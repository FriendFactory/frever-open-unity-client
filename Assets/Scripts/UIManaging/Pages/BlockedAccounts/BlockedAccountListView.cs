using System.Collections.Generic;
using Bridge;
using Bridge.Services.UserProfile;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.BlockedAccounts
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    internal sealed class BlockedAccountListView: MonoBehaviour
    {
        [Inject] private IStorageBridge _storageBridge;
        [SerializeField] private BlockedAccountView _blockedAccountPrefab;
        private readonly List<BlockedAccountView> _displayedViews = new List<BlockedAccountView>();

        public void Display(Profile[] profiles)
        {
            Clear();
            SpawnProfileUiViews(profiles);
        }

        private void SpawnProfileUiViews(Profile[] profiles)
        {
            foreach (var profile in profiles)
            {
                var profileView = Instantiate(_blockedAccountPrefab, transform);
                profileView.Display(profile);
                _displayedViews.Add(profileView);
            }
        }

        private void Clear()
        {
            foreach (var accountView in _displayedViews)
            {
                accountView.Destroy();
            }
            _displayedViews.Clear();
        }

        void OnDisable()
        {
            Clear();
        }
    }
}