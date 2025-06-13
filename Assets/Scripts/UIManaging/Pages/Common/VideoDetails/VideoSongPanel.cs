using System;
using Bridge.Models.VideoServer;
using Common.Abstract;
using Extensions;
using I2.Loc;
using UIManaging.Pages.Common.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.VideoDetails
{
    public abstract class VideoSongPanel: BaseContextPanel<Video>
    {
        [SerializeField] private ScrollableSongNamePanel _scrollableSongNamePanel;
        [SerializeField] private Button _useButton;
        [Header("L10N")] 
        [SerializeField] private LocalizedString _defaultUserSoundNameLoc;

        public void StartScrolling() => _scrollableSongNamePanel.StartScrolling();
        public void StopScrolling() => _scrollableSongNamePanel.StopScrolling();

        protected bool Interactable
        {
            get => _useButton.interactable;
            set => _useButton.interactable = value;
        }
        
        private SongTextModel SongTextModel { get; set; }

        public event Action Clicked;

        protected override void OnInitialized()
        {
            PrepareSongNameText();

            _useButton.onClick.AddListener(OnClicked);
        }

        protected override void BeforeCleanUp()
        {
            _scrollableSongNamePanel.CleanUp();
            
            _useButton.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            Clicked?.Invoke();
            
            MoveNext();
        }

        protected abstract void MoveNext();

        private void PrepareSongNameText()
        {
            if (!ContextData.HasMusic())
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            
            SongTextModel =  new SongTextModel(ContextData);
            
            if (string.IsNullOrEmpty(SongTextModel.Text) && ContextData.UserSounds.Length > 0)
            {
                SongTextModel = new SongTextModel(_defaultUserSoundNameLoc);
            }
            
            _scrollableSongNamePanel.Initialize(SongTextModel);
        }
    }
}