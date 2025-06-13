using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using Common.BridgeAdapter;
using Common.Publishers;
using JetBrains.Annotations;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.FollowersPage.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui.ProfileHelper
{
    internal abstract class BaseUserProfileHelper : MonoBehaviour
    {
        [SerializeField] protected GameObject[] _relatedGameObjects;
        [SerializeField] protected GameObject[] _buttons;
        [SerializeField] protected HorizontalLayoutGroup _horizontalLayout;

        protected IPublishVideoHelper PublishVideoHelper;
        protected IBridge Bridge;
        protected ILevelService LevelService;
        protected PageManager PageManager;
        protected VideoManager VideoManager;
        protected UserProfilePage ProfilePage;
        protected FollowersManager FollowersManager;

        private BaseVideoListLoader _levelsPanelArgs;
        protected UserTaggedVideoListLoader TaggedLevelPanelArgs;

        public Profile Profile { get; private set; }
        public ProfileKPI ProfileKPI => Profile.KPI;
        
        public abstract bool IsCurrentUser { get; }
        protected abstract long UserGroupId { get; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(IPublishVideoHelper publishVideoHelper, IBridge bridge, PageManager pageManager, VideoManager videoManager, FollowersManager followersManager, 
                               ILevelService levelService)
        {
            PublishVideoHelper = publishVideoHelper;
            Bridge = bridge;
            PageManager = pageManager;
            VideoManager = videoManager;
            FollowersManager = followersManager;
            LevelService = levelService;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            ProfilePage = GetComponent<UserProfilePage>();
        }

        protected abstract void OnEnable();

        protected abstract void OnDisable();

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task<bool> LoadUserProfileAsync(CancellationToken token = default)
        {
            var result = await Bridge.GetProfile(UserGroupId, token);
            if (result.IsSuccess)
            {
                Profile = result.Profile;
                return true;
            }

            if (!result.IsRequestCanceled)
            {
                Debug.LogError($"Failed to load user profile. [Reason]: {result.ErrorMessage}");
            }

            return false;
        }

        public BaseVideoListLoader InitLevelsPanelArgs()
        {
            _levelsPanelArgs = CreateLevelsPanelArgs();
            return _levelsPanelArgs;
        }
        
        public abstract UserTaggedVideoListLoader InitTaggedLevelPanelArgs();

        public abstract BaseFollowersPageArgs GetFollowersPageArgs(int tabIndex);

        public abstract UserProfileTaskVideosGridLoader InitTaskLevelPanelArgs();
        
        public async void RefreshKPIView(CancellationToken token = default)
        {
            if (await LoadUserProfileAsync(token))
            {
                ProfilePage.ProfileKpiView.Initialize(Profile.KPI);
            }
        }

        public virtual void UpdateRelatedUI(bool show)
        {
            foreach (var button in _buttons)
            {
                button.SetActive(show);
            }
            foreach (var relatedGameObject in _relatedGameObjects)
            {
                relatedGameObject.SetActive(show);
            }
        }

        public void CleanUp()
        {
            _levelsPanelArgs = null;
        }

        public void ForceUpdateLayouts()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_horizontalLayout.transform as RectTransform);
        }
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected abstract BaseVideoListLoader CreateLevelsPanelArgs();
    }
}