using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Common.SelectionPanel;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.UserSelection
{
    public class UserSelectionItemView: SelectionItemView<UserSelectionItemModel>
    {
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private TextMeshProUGUI _userNickname;
        [SerializeField] private GameObject _crossObj;
        [SerializeField] private CanvasGroup _itemGroup;
        [SerializeField] private float _lockedAlpha = 0.5f;

        [Inject] private SnackBarHelper _snackBarHelper;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            _crossObj.SetActive(!ContextData.IsLocked);
            _itemGroup.alpha = ContextData.IsLocked ? _lockedAlpha : 1;
            
            if (!ContextData.ShortProfile.MainCharacterId.HasValue)
            {
                Debug.LogError($"Missing main character id for user {ContextData.ShortProfile.Nickname}");
                return;
            }
            
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = ContextData.ShortProfile.Id,
                UserMainCharacterId = ContextData.ShortProfile.MainCharacterId.Value,
                MainCharacterThumbnail = ContextData.ShortProfile.MainCharacterFiles
            };
            
            _userPortraitView.Initialize(userPortraitModel);
            _userNickname.text = string.IsNullOrWhiteSpace(ContextData.ShortProfile.Nickname) ? "<Name Is Null>" : ContextData.ShortProfile.Nickname;

            ContextData.SelectionChangeLocked += OnSelectionChangeLocked;
        }

        protected override void BeforeCleanup()
        {
            ContextData.SelectionChangeLocked -= OnSelectionChangeLocked;
            
            _userPortraitView.CleanUp();
            
            base.BeforeCleanup();
        }

        private void OnSelectionChangeLocked()
        {
            _snackBarHelper.ShowInformationDarkSnackBar("Can't remove tagged users");
        }
    }
}