using System;
using Bridge;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.FavoriteSounds
{
    public class VideosBasedOnSoundPage: GenericPage<VideosBasedOnSoundPageArgs>
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private UsedSoundPanel _usedSoundPanel;
        [SerializeField] private UseFavoriteSoundButton _useFavoriteSoundButton;
        [SerializeField] private VideoList _videosGrid;
 
        [Inject] private PageManager _pageManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private IBridge _bridge;
        
        public override PageId Id => PageId.VideosBasedOnSound;
        
        protected override void OnInit(PageManager pageManager) { }

        protected override void OnDisplayStart(VideosBasedOnSoundPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _pageHeaderView.Init(new PageHeaderArgs(string.Empty, new ButtonArgs(string.Empty, MoveBack)));

            var model = args.UsedSoundModel;
            var sound = model.Sound;
            
            _usedSoundPanel.Initialize(model);
            _useFavoriteSoundButton.Initialize(sound);

            _videosGrid.Initialize(args.GetVideoListLoader(_pageManager, _videoManager, _bridge));
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            _usedSoundPanel.CleanUp();
            _useFavoriteSoundButton.CleanUp();
        }

        private void MoveBack() => _pageManager.MoveBack();
    }
}