using System;
using System.Threading;
using Common.Abstract;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Common.Args.Views.Profile
{
    public class LocalUserPortraitView: BaseContextlessPanel
    {
        [SerializeField] private UserPortrait _userPortrait;
        [SerializeField] private Resolution _resolution = Resolution._128x128;

        private LocalUserDataHolder _localUserDataHolder;
        private CancellationTokenSource _cancellationTokenSource;

        [Inject, UsedImplicitly]
        private void Construct(LocalUserDataHolder localUserDataHolder)
        {
            _localUserDataHolder = localUserDataHolder;
        }

        protected override async void OnInitialized()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                await _userPortrait.InitializeAsync(_localUserDataHolder.UserProfile, _resolution, _cancellationTokenSource.Token);
                
                _userPortrait.ShowContent();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        protected override void BeforeCleanUp()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            
            _userPortrait.CleanUp();
        }
    }
}