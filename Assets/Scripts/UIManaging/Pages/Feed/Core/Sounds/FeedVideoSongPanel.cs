using System;
using System.Linq;
using Bridge;
using Extensions;
using Navigation.Core;
using UIManaging.Pages.FavoriteSounds;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.UsedVideoSounds;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.VideoDetails
{
    public class FeedVideoSongPanel: VideoSongPanel
    {
        private const string FAILED_TO_LOAD_SOUND_MESSAGE = "Sorry, this music is not available in your country";

        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackbarHelper;
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;

        private UsedVideoSoundsLoader _videoSoundsLoader;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _videoSoundsLoader = new UsedVideoSoundsLoader(_bridge, ContextData);
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            _videoSoundsLoader?.Dispose();
        }

        protected override async void MoveNext()
        {
            Interactable = false;
            
            try
            {
                if (!ContextData.HasMusic()) return;

                var sounds = await _videoSoundsLoader.GetVideoSoundsAsync();

                if (sounds.IsNullOrEmpty())
                {
                    _snackbarHelper.ShowFailSnackBar(FAILED_TO_LOAD_SOUND_MESSAGE);
                    return;
                }

                if (sounds.Count > 1)
                {
                    _popupManager.SetupPopup(new UsedVideoSoundsPopupConfiguration(sounds));
                    _popupManager.ShowPopup(PopupType.UsedVideoSounds);
                }
                else
                {
                    var sound = sounds.First();
                    
                    _pageManager.MoveNext(new VideosBasedOnSoundPageArgs(sound));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Interactable = true;
            }
        }
    }
}