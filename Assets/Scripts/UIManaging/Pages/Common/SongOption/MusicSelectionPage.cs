using System;
using Bridge.Models.Common;
using BrunoMikoski.AnimationSequencer;
using Extensions;
using Modules.InputHandling;
using Navigation.Core;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.Common.SongOption.Search;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    public class MusicSelectionPage : GenericPage<MusicSelectionPageArgs>
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private MusicGalleryView _musicGalleryView;
        [SerializeField] private Button _closeButton;
        [SerializeField] private AnimationSequencerController _animationSequencer;
        [SerializeField] private MusicSearchHandler _musicSearchHandler;
        [SerializeField] private SkipMusicSelectionButton _skipMusicSelectionButton;

        [Inject] private SongSelectionController _songSelectionController;
        [Inject] private MusicSelectionPageModel _pageModel;
        [Inject] private MusicSelectionStateController _musicSelectionStateController;
        [Inject] private IInputManager _inputManager;

        private bool _parentPageInputEnabled;

        public override PageId Id => PageId.MusicSelection;
        
        //---------------------------------------------------------------------
        // Page 
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            _musicSelectionStateController.Initialize();
            
            _pageModel.SkipRequested += OnSkipRequested;
        }

        protected override void OnDisplayStart(MusicSelectionPageArgs args)
        {
            base.OnDisplayStart(args);
            
            // responsible for grid row snapping through drag gesture recognition
            _parentPageInputEnabled = _inputManager.Enabled;
            _inputManager.Enable(true);
            
            _animationSequencer.PlayForward();
            
            _songSelectionController.SongApplied += OnSongApplied;
            _songSelectionController.IsSelectionForEventRecording = args.SelectionPurpose == SelectionPurpose.ForRecordingNewEvent;
            _closeButton.onClick.AddListener(OnClose);

            if (!_musicGalleryView.IsInitialized)
            {
                _musicGalleryView.Initialize();
            }
            _musicGalleryView.Show();
            
            _skipMusicSelectionButton.SetActive(_pageModel.SkipAllowed);
            
            _pageModel.OnPageOpened();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            _inputManager.Enable(_parentPageInputEnabled);
            
            _songSelectionController.SongApplied -= OnSongApplied;
            
            _closeButton.onClick.RemoveListener(OnClose);
            
            _musicGalleryView.Hide();

            if (_musicSearchHandler.IsInitialized)
            {
                _musicSearchHandler.CleanUp();
            }
            
            _pageModel.OnPageClosed();
        }

        protected override void OnCleanUp()
        {
            _pageModel.SkipRequested -= OnSkipRequested;
            
            _animationSequencer.Kill();
        }

        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private void OnClose()
        {
            // show LE UI/PiP before playing an animation
            _pageModel.OnPageCloseRequested();

            _canvasGroup.interactable = false;
            
            _animationSequencer.PlayBackwards(true, () => {
                _canvasGroup.interactable = true;
                
                Hide();
            });
        }

        private void OnSongApplied(IPlayableMusic song, int activationCue)
        {
            if (song == null) return;
            
            _pageModel.OnSongApplied(song, activationCue);
            OnClose();
        }

        private void OnSkipRequested() => OnClose();
    }
}