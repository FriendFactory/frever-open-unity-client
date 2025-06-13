using System;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.FavoriteSounds
{
    public class SavedSoundsPage: GenericPage<SavedSoundsPageArgs>
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private SavedSoundsPanel _savedSoundsPanel;
        
        [Inject] private PageManager _pageManager;
        [Inject] private MusicPlayerController _musicPlayerController;
        
        public override PageId Id => PageId.SavedSounds;

        protected override void OnInit(PageManager pageManager)
        {
            var header = _pageHeaderView.Header;
            _pageHeaderView.Init(new PageHeaderArgs(header, new ButtonArgs(string.Empty, MoveBack)));
        }

        protected override void OnDisplayStart(SavedSoundsPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _savedSoundsPanel.Initialize();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            _musicPlayerController.Stop();
            
            _savedSoundsPanel.CleanUp();
        }

        private void MoveBack() => _pageManager.MoveBack();
    }
}