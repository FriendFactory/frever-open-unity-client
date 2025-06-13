using System;
using System.Threading;
using Abstract;
using Bridge.ClientServer.Assets.Music;
using Navigation.Core;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.FavoriteSounds
{
    public class OpenSavedSoundsButton: BaseContextDataButton<long>
    {
        private const string FAILED_TO_GET_FAVORITE_SOUNDS_MESSAGE = "Failed to get Saved sounds";
        private const int TAKE_NEXT_COUNT = 50;
        
        [Inject] private IFavouriteMusicService _favouriteMusicService;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PageManager _pageManager;

        private CancellationTokenSource _downloadTokenSource;
         
        protected override void OnInitialized() { }

        protected override void BeforeCleanup()
        {
            Cancel();
        }

        protected override void OnUIInteracted()
        {
            base.OnUIInteracted();
            
            OpenSavedSoundsPage();
        }

        private void OpenSavedSoundsPage()
        {
            try
            {
                Cancel();

                _downloadTokenSource = new CancellationTokenSource();
                
                _button.interactable = false;
                
                _pageManager.MoveNext(new SavedSoundsPageArgs());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _button.interactable = true;
            }
        }

        private void Cancel()
        {
            if (_downloadTokenSource == null) return;
            
            _downloadTokenSource.Cancel();
            _downloadTokenSource = null;
        }
    }
}