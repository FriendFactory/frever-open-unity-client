using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Common;
using Common.BridgeAdapter;
using Extensions;
using Models;
using Modules.InputHandling;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews
{
    public class CrewChatVideoSelectionPanel : MonoBehaviour
    {
        private const int USER_VIDEOS_TAB_INDEX = 0;
        private const int USER_TASKS_TAB_INDEX = 1;
        private const int USER_TAGGED_TAB_INDEX = 2;

        [SerializeField] private VideosTabsManagerView _tabsManager;
        [Space]
        [SerializeField] private VideoGrid[] _videosGrids;
        [Space]
        [SerializeField] private Button _outsideButton;

        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private ILevelService _levelService;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private IBackButtonEventHandler _backButtonEventHandler;

        private bool _isInitialized;
        private CancellationTokenSource _cancellationSource;

        private TabModel[] _tabModels;
        private Dictionary<int, bool> _videoTabLoadState;

        private Action<Video, Texture2D> _onVideoSelected;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _tabsManager.TabSelectionCompleted += OnTabSelected;
            _outsideButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            _tabsManager.TabSelectionCompleted -= OnTabSelected;
            _outsideButton.onClick.RemoveListener(Hide);
        }

        private void OnDestroy()
        {
            _onVideoSelected = null;
            CancelThumbnailLoading();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(Action<Video, Texture2D> onVideoSelected)
        {
            if (!_isInitialized) Initialize();

            _onVideoSelected = onVideoSelected;
            gameObject.SetActive(true);
            
            _backButtonEventHandler.AddButton(_outsideButton.gameObject, Hide);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onVideoSelected = null;
            _backButtonEventHandler.RemoveButton(_outsideButton.gameObject);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Initialize()
        {
            InitializeTabs($"<sprite=1> {GetNonDraftLevelsCount()}",
                           $"<sprite=0> {GetTaggedInVideoCount()}",
                           $"<sprite=2> {GetTaskVideoCount()}");

            OnTabSelected(USER_VIDEOS_TAB_INDEX);
            _isInitialized = true;
        }

        private void InitializeTabs(string postsTabName, string taggedTabName, string taskTabName)
        {
            InitVideoTabLoadState();

            _tabModels = new[]
            {
                new TabModel(USER_VIDEOS_TAB_INDEX, postsTabName),
                new TabModel(USER_TASKS_TAB_INDEX, taskTabName),
                new TabModel(USER_TAGGED_TAB_INDEX, taggedTabName),
            };

            _tabsManager.gameObject.SetActive(true);
            _tabsManager.Init(new TabsManagerArgs(_tabModels, USER_VIDEOS_TAB_INDEX));
        }

        private void InitVideoTabLoadState()
        {
            _videoTabLoadState = new Dictionary<int, bool>
            {
                {USER_VIDEOS_TAB_INDEX, false},
                {USER_TASKS_TAB_INDEX, false},
                {USER_TAGGED_TAB_INDEX, false}
            };
        }

        private void OnTabSelected(int tabIndex)
        {
            for (var i = 0; i < _videosGrids.Length; i++)
            {
                _videosGrids[i].SetActive(i == tabIndex);
            }

            LoadTabIfNeeded(tabIndex);
        }

        private void LoadTabIfNeeded(int tabIndex)
        {
            if (IsTabLoaded(tabIndex)) return;

            var videoLoader = GetVideoLoader(tabIndex);
            _videosGrids[tabIndex].Initialize(videoLoader);

            SetTabAsLoaded(tabIndex);
        }

        private BaseVideoListLoader GetVideoLoader(int tabIndex)
        {
            switch (tabIndex)
            {
                case USER_VIDEOS_TAB_INDEX:
                    return new UserPublicVideoListLoader(_videoManager, _pageManager, _bridge, OnVideoSelected);
                case USER_TASKS_TAB_INDEX:
                    return new UserTaskVideoListLoader(_videoManager, _pageManager, _bridge, OnVideoSelected);
                case USER_TAGGED_TAB_INDEX:
                    return new UserTaggedVideoListLoader(_videoManager, _pageManager, _bridge, OnVideoSelected);
                default:
                    throw new NotImplementedException();
            }
        }

        private bool IsTabLoaded(int tabIndex)
        {
            return _videoTabLoadState[tabIndex];
        }

        private void SetTabAsLoaded(int tabIndex)
        {
            _videoTabLoadState[tabIndex] = true;
        }

        private async void OnVideoSelected(BaseLevelItemArgs args)
        {
            CancelThumbnailLoading();

            if (!args.Video.LevelId.HasValue)
            {
                _onVideoSelected?.Invoke(args.Video, null);
                Hide();
            }
            else
            {
                _cancellationSource = new CancellationTokenSource();

                var level = await DownloadFullLevelData(args.Video.LevelId.Value, _cancellationSource.Token);

                if (level != null)
                {
                    _videoManager.GetThumbnailForLevel(level, Resolution._128x128, OnThumbnailDownloaded, _cancellationSource.Token);
                }
                else
                {
                    OnThumbnailDownloaded(null);
                }

                void OnThumbnailDownloaded(Texture2D thumbnail)
                {
                    _onVideoSelected?.Invoke(args.Video, thumbnail);
                    Hide();
                }
            }
        }

        private async Task<Level> DownloadFullLevelData(long levelId, CancellationToken cancellationToken)
        {
            var result = await _levelService.GetLevelAsync(levelId, cancellationToken);

            if (result.IsSuccess) return result.Level;

            if (result.ErrorMessage != null &&
                result.ErrorMessage.Contains(Constants.ErrorMessage.ASSET_INACCESSIBLE_IDENTIFIER))
            {
                Debug.LogWarning("Cannot open because assets are no longer accessible");
            }

            return null;
        }

        private void CancelThumbnailLoading()
        {
            _cancellationSource?.Cancel();
            _cancellationSource = null;
        }

        private int GetNonDraftLevelsCount()
        {
            var profileKpi = _userData.UserProfile.KPI;
            return profileKpi.TotalVideoCount - profileKpi.TaskVideoCount;
        }

        private int GetTaggedInVideoCount()
        {
            return _userData.UserProfile.KPI.TaggedInVideoCount;
        }

        private int GetTaskVideoCount()
        {
            return _userData.UserProfile.KPI.TaskVideoCount;
        }
    }
}