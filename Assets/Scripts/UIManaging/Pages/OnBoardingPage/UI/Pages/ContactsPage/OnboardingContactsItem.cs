using Bridge;
using Bridge.Services.UserProfile;
using Common.Abstract;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Common.SelectionPanel;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    public class OnboardingContactsItem: BaseContextPanel<OnboardingContactsItemModel>
    {
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private SelectionCheckmarkView _selectionCheckmarkView;
        [SerializeField] private TMP_Text _realName;
        [SerializeField] private TMP_Text _nickname;

        [Inject] private IBridge _bridge;

        protected override bool IsReinitializable => true;

        protected override void OnInitialized()
        {
            _realName.text = ContextData.RealName;
            _nickname.text = ContextData.Profile.NickName;
            
            _selectionCheckmarkView.Initialize(ContextData);
            
            InitializeUserPortrait(ContextData.Profile);
        }

        protected override void BeforeCleanUp()
        {
            _selectionCheckmarkView.CleanUp();
            _userPortraitView.CleanUp();

            _realName.text = string.Empty;
            _nickname.text = string.Empty;
        }

        private void InitializeUserPortrait(Profile profile)
        {
            if (profile?.MainCharacter == null) return;

            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = profile.MainGroupId,
                UserMainCharacterId = profile.MainCharacter.Id,
                MainCharacterThumbnail = profile.MainCharacter.Files,
            };
            
            _userPortraitView.Initialize(userPortraitModel);
        }
    }
}