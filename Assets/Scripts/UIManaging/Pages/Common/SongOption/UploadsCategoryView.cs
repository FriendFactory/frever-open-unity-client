using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Extensions;
using Modules.CharacterManagement;
using UIManaging.Localization;
using UIManaging.Pages.Common.SongOption.Uploads;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal class UploadsCategoryView: MusicViewBase<MusicViewModel>
    {
        private const int OWN_SOUNDS_TAB_INDEX = 0;
        private const int PUBLIC_SOUNDS_TAB_INDEX = 1;
        
        [SerializeField] private TabsManagerView _tabsManagerView;
        [SerializeField] private UploadsOwnSoundsView _ownSoundsView;
        [SerializeField] private UploadsPublicSoundsView _publicSoundsView;
        
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private CharacterManager _characterManager;
        [Inject] private IBridge _bridge;
        [Inject] private MusicGalleryLocalization _localization;
        
        private UserSoundsListModel _ownSoundsListModel;
        private TrendingUserSoundsListModel  _trendingSoundsListModel;

        protected override string Name => _localization.UploadsCategoryHeader;

        public override async Task InitializeAsync(MusicViewModel model, CancellationToken token)
        {
            _ownSoundsListModel = new UserSoundsListModel(_bridge, _localUserDataHolder, _characterManager);
            _trendingSoundsListModel = new TrendingUserSoundsListModel(_bridge);

            var tabModels = new[]
            {
                new TabModel(OWN_SOUNDS_TAB_INDEX, _localization.UploadsMySoundsTab),
                new TabModel(PUBLIC_SOUNDS_TAB_INDEX, _localization.UploadsPublicSoundsTab),
            };

            _tabsManagerView.Init(new TabsManagerArgs(tabModels));

            await _ownSoundsListModel.InitializeAsync(token);
            
            _ownSoundsView.Initialize(_ownSoundsListModel);
            _publicSoundsView.Initialize(_trendingSoundsListModel);

            await base.InitializeAsync(model, token);
        }

        protected override void OnCleanUp()
        {
            _ownSoundsListModel.CleanUp();
            _trendingSoundsListModel.Reset();
        }

        protected override void OnActivated()
        {
            // add handling for scroll out of bounds release
            // https://github.com/FriendFactory/Frever/blob/develop/Assets/Scripts/UIManaging/Pages/Common/SongOption/SongSelectionMenu.cs#L573

            _tabsManagerView.TabSelectionCompleted += OnTabSelectionCompleted;
        }

        protected override void OnDeactivated()
        {
            _tabsManagerView.TabSelectionCompleted -= OnTabSelectionCompleted;
        }

        protected override void OnShowContent()
        {
            base.OnShowContent();
            
            _tabsManagerView.TabsManagerArgs.SetSelectedTabIndex(OWN_SOUNDS_TAB_INDEX);
            ShowOwnSounds();
        }

        private void OnTabSelectionCompleted(int index)
        {
            switch (index)
            {
                case OWN_SOUNDS_TAB_INDEX:
                    ShowOwnSounds();
                    break;
                case PUBLIC_SOUNDS_TAB_INDEX:
                    ShowPublicSounds();
                    break;
            }
        }

        private void ShowOwnSounds()
        {
            _publicSoundsView.SetActive(false);
            _ownSoundsView.SetActive(true);
        }
        
        private void ShowPublicSounds()
        {
            _ownSoundsView.SetActive(false);
            _publicSoundsView.SetActive(true);
        }
    }
}