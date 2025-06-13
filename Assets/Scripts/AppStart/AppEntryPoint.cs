using System;
using System.Collections;
using Bridge;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AppStart
{
    internal sealed class AppEntryPoint : MonoBehaviour
    {
        [SerializeField] private int _startupSceneIndex = 1;
        [SerializeField] private Animation _animation;
        [SerializeField] private VideoPlayerCanvas _videoPlayerCanvasPrefab;
        [SerializeField] private LoadingProgressView _loadingProgressViewPrefab;
        private bool _autologinAttemptCompleted;
        private LoadingProgressView _loadingProgressView;
        
        private IEnumerator Start()
        {
            var bridge = new ServerBridge();
            var criticalDataFetcher = new CriticalDataFetcher(bridge);
            var userProfileLoader = new UserProfileFetcher(bridge);
            AppEntryContext.UserProfileFetcher = userProfileLoader;
            StartLocalizationDataFetching(bridge);
            AppEntryContext.OnLoadingStarted = () =>
            {
                if (_loadingProgressView == null)
                {
                    SpawnLoadingProgressView();
                }
                if (!_loadingProgressView.isActiveAndEnabled)
                {
                    ShowLoadingProgressView();
                }
            };

            TryAutoLogin(bridge, () =>
            {
                criticalDataFetcher.Fetch();
                userProfileLoader.Fetch();
                SpawnLoadingProgressView();
            }, () =>
            {
                #if UNITY_EDITOR
                Debug.Log($"Editor mode: Switch to Stage environment by default");
                bridge.ChangeEnvironment(FFEnvironment.Stage);
                #endif
                
                //start fetch video
                var videoPlayerCanvas = Instantiate(_videoPlayerCanvasPrefab);
                videoPlayerCanvas.Prepare();
                videoPlayerCanvas.SetTransparency(0);
                AppEntryContext.VideoPlayerCanvas = videoPlayerCanvas;
            });
            _animation.Play();
            var mainSceneLoadOperation = SceneManager.LoadSceneAsync(_startupSceneIndex);
            mainSceneLoadOperation.allowSceneActivation = false;
            while (_animation.isPlaying || !_autologinAttemptCompleted)
            {
                yield return null;
            }

            if (AppEntryContext.State == EntryState.NotLoggedIn)
            {
                //show frame and stop playing because of glitches on loading Main scene where we have all services initialzation
                AppEntryContext.VideoPlayerCanvas.Play();
                AppEntryContext.VideoPlayerCanvas.Pause();
                if (AppEntryContext.VideoPlayerCanvas.IsVideoReady)
                {
                    AppEntryContext.VideoPlayerCanvas.SetTransparency(0.5f);
                    AppEntryContext.VideoPlayerCanvas.SetTransparency(1, 0.5f);
                    AppEntryContext.VideoPlayerCanvas.ShowLoadingIndicator();
                }
                else
                {
                    AppEntryContext.VideoPlayerCanvas.VideoReady += ()=> AppEntryContext.VideoPlayerCanvas.SetTransparency(1);
                }
            }

            if (AppEntryContext.State == EntryState.LoggedIn)
            {
                ShowLoadingProgressView();
            }
            
            AppEntryContext.Bridge = bridge;
            AppEntryContext.CriticalDataFetcher = criticalDataFetcher;
            mainSceneLoadOperation.allowSceneActivation = true;
        }

        private static async void StartLocalizationDataFetching(IBridge bridge)
        {
            AppEntryContext.LocalizationSetup = new LocalizationSetup(bridge);;
            await AppEntryContext.LocalizationSetup.FetchLocalization(false);
        }

        private void SpawnLoadingProgressView()
        {
            _loadingProgressView = Instantiate(_loadingProgressViewPrefab);
            _loadingProgressView.gameObject.SetActive(false);
        }

        private void ShowLoadingProgressView()
        {
            _loadingProgressView.gameObject.SetActive(true);
            _loadingProgressView.PlayFakeProgress(1.5f);
            AppEntryContext.OnLoadingDone = DestroyEntryViews;
        }

        private void DestroyEntryViews()
        {
            if (_loadingProgressView != null)
            {
                Destroy(_loadingProgressView.gameObject);
                _loadingProgressView = null;
            }

            if (AppEntryContext.VideoPlayerCanvas != null)
            {
                Destroy(AppEntryContext.VideoPlayerCanvas.gameObject);
                AppEntryContext.VideoPlayerCanvas = null;
            }
        }
        
        private async void TryAutoLogin(IBridge bridge, Action onLoggedIn, Action onLoginRequired)
        {
            var targetEnvironment = bridge.LastLoggedEnvironment ?? FFEnvironment.Production;
            var compatibility = await bridge.GetEnvironmentCompatibilityData(targetEnvironment);

            if (compatibility.IsError)
            {
                AppEntryContext.State = EntryState.NoInternetConnection;
                _autologinAttemptCompleted = true;
                return;
            }
            
            if (!compatibility.IsCompatibleWithBridge)
            {
                AppEntryContext.State = EntryState.OutdatedApp;
                _autologinAttemptCompleted = true;
                return;
            }

            var resp = await bridge.LoginToLastSavedUserAsync();
            if (resp.IsError)
            {
                AppEntryContext.State = EntryState.NotLoggedIn;
                onLoginRequired?.Invoke();
                _autologinAttemptCompleted = true;
                return;
            }

            AppEntryContext.State = EntryState.LoggedIn;
            onLoggedIn?.Invoke();
            _autologinAttemptCompleted = true;
        }
    }
}
