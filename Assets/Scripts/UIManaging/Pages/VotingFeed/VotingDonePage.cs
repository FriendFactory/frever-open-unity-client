using System;
using System.Collections.Generic;
using Navigation.Core;
using TMPro;
using UIManaging.Common;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Common.InputFields;
using UIManaging.Pages.PublishPage;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.VotingFeed
{
    public class VotingDonePage : GenericPage<VotingDonePageArgs>
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _nicknameText;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private DressCodeItem _dressCodeItemPrefab;
        [SerializeField] private Transform _dressCodeItemContainer;
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private LevelThumbnail _levelThumbnail;
        
        [Inject] private IPublishVideoController _publishVideoController;

        private readonly List<DressCodeItem> _dressCodeItems = new List<DressCodeItem>();
        
        public override PageId Id => PageId.VotingDone;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            
        }
        
        protected override void OnDisplayStart(VotingDonePageArgs args)
        {
            base.OnDisplayStart(args);
            
            _publishVideoController.SubscribeToEvents();
            
            _backButton.onClick.AddListener(OnBackButton);
            _continueButton.onClick.AddListener(OnContinueButton);

            _titleText.text = args.Task.Name;

            if (args.DressCodes == null || args.DressCodes.Count == 0)
            {
                _descriptionText.text = args.Task.Description;
            }
            else
            {
                _descriptionText.text = "";
                
                foreach (var dressCode in args.DressCodes)
                {
                    var dressCodeItem = Instantiate(_dressCodeItemPrefab, _dressCodeItemContainer);
                    dressCodeItem.SetName(dressCode);
                
                    _dressCodeItems.Add(dressCodeItem);
                }
            }
            
            RefreshPortraitView();
            PrepareUserNicknameText();
            
            _levelThumbnail.Initialize(args.CreatedLevel);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _publishVideoController.UnsubscribeFromEvents();
            
            _backButton.onClick.RemoveListener(OnBackButton);
            _continueButton.onClick.RemoveListener(OnContinueButton);

            foreach (var item in _dressCodeItems)
            {
                Destroy(item);
            }
            
            _dressCodeItems.Clear();
            _userPortraitView.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        private void OnBackButton()
        {
            OpenPageArgs.MoveBack?.Invoke();
        }

        private void OnContinueButton()
        {
            OpenPageArgs.PublishStart?.Invoke();
        }
        
        private void RefreshPortraitView()
        {
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = OpenPageArgs.CreatedLevel.GroupId,
                UserMainCharacterId = OpenPageArgs.MainCharacterId,
                MainCharacterThumbnail = OpenPageArgs.MainCharacterThumbnail
            };

            _userPortraitView.Initialize(userPortraitModel);
        }

        private void PrepareUserNicknameText()
        {
            var nickName = AdvancedInputFieldUtils.GetParsedText(OpenPageArgs.UserNickname);

            _nicknameText.text = nickName;
        }
    }
}