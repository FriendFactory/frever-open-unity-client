using System.Threading;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Battles;
using Bridge.Models.VideoServer;
using Extensions;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.VotingResult
{
    public sealed class VotingResultModel
    {
        public readonly BattleResult BattleResult;
        public readonly int Place;

        public VotingResultModel(BattleResult battleResult, int place)
        {
            BattleResult = battleResult;
            Place = place;
        }
    }
    
    internal sealed class UserVotingResultView: BaseContextDataView<VotingResultModel>
    {
        [SerializeField] private UserPlaceView _userPlaceView;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private TMP_Text _userNickname;
        [SerializeField] private GameObject _highlightObject;
        [SerializeField] private TMP_Text _rewards;
        [SerializeField] private Button _button;
        
        [Inject] private IBridge _bridge;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private IBlockedAccountsManager _blockedAccountsManager;

        private GroupInfo Group => ContextData.BattleResult.Group;
        private bool IsLocalUser => Group.Id == _bridge.Profile.GroupId;

        private CancellationTokenSource _tokenSource;
        
        private void Awake()
        {
            _button.onClick.AddListener(OnClicked);
        }

        protected override void OnInitialized()
        {
            _tokenSource = new CancellationTokenSource();
            _userPlaceView.Setup(ContextData.Place);
            _scoreText.text = ContextData.BattleResult.Score.ToString("N1");
            _highlightObject.SetActive(IsLocalUser);
            SetupUserName();
            _rewards.text = ContextData.BattleResult.SoftCurrency.ToString();
            ShowUserImage();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _tokenSource.CancelAndDispose();
        }

        private void SetupUserName()
        {
            _userNickname.text = IsLocalUser? "Me" : Group.Nickname;
        }

        private void ShowUserImage()
        {
            var userModel = new UserPortraitModel
            {
                UserGroupId = Group.Id,
                UserMainCharacterId = Group.MainCharacterId.Value,
                MainCharacterThumbnail = Group.MainCharacterFiles,
                Resolution = Resolution._128x128
            };
            _userPortraitView.Initialize(userModel);
        }

        private void OnClicked()
        {
            if (IsLocalUser)
            {
                return;
            }

            if (_blockedAccountsManager.IsUserBlocked(Group.Id))
            {
                _snackBarHelper.ShowProfileBlockedSnackBar();
                return;
            }
            
            _popupManagerHelper.ShowFollowAccountPopup(Group.Id, _tokenSource.Token);
        }
    }
}