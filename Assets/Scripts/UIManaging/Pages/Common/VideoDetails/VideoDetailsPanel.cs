using System;
using Bridge.Models.VideoServer;
using Common.Abstract;
using Extensions;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoDetails.VideoAttributes;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Common.VideoDetails
{
    public class VideoDetailsPanel: BaseContextPanel<VideoDetailsModel>
    {
        [SerializeField] private VideoAttributesPanel _videoAttributesPanel;
        [SerializeField] private UserPortraitView _userPortraitView; 
        [SerializeField] private NicknameView _nicknameView; 
        [SerializeField] private VideoSongPanel _musicPanel;
        [SerializeField] private FeedVideoDescription _descriptionPanel;
        
        [Inject] private LocalUserDataHolder _localUserDataHolder;

        private Video Video => ContextData.Video;

        public event Action<VideoAttributeType> VideoAttributeClicked;
            
        protected override void OnInitialized()
        {
            _videoAttributesPanel.Initialize(new VideoAttributesModel(Video, null, 0, generateTemplateWithName: ContextData.GenerateTemplateWithName));

            _videoAttributesPanel.AttributeClicked += OnVideoAttributeClicked;
            
            _descriptionPanel.Init(Video);
            _nicknameView.Initialize(new NicknameModel
            {
                Nickname = _localUserDataHolder.NickName
            });
            
            RefreshPortraitView();
            
            var containsMusic = !string.IsNullOrEmpty(Video.SongName);

            _musicPanel.SetActive(containsMusic);

            if (containsMusic)
            {
                _musicPanel.Initialize(Video);
                _musicPanel.StartScrolling();
            }
        }

        protected override void BeforeCleanUp()
        {
            _videoAttributesPanel.AttributeClicked -= OnVideoAttributeClicked;
            
            _videoAttributesPanel.CleanUp();
            _descriptionPanel.BeforeCleanup();
            _nicknameView.CleanUp();
            
            if (_musicPanel.IsInitialized)
            {
                _musicPanel.CleanUp();
            }
        }
        
        private void RefreshPortraitView()
        {
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = Video.GroupId,
                UserMainCharacterId = Video.Owner.MainCharacterId.Value,
                MainCharacterThumbnail = Video.Owner.MainCharacterFiles
            };

            _userPortraitView.Initialize(userPortraitModel);
        }
        
        private void OnVideoAttributeClicked(VideoAttributeType type) => VideoAttributeClicked?.Invoke(type);
    }
}