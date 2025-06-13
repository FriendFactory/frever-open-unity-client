using Common;
using OldMoatGames;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal class AssetSelectionAnimatedGifItemView : AssetSelectionAnimatedItemView
    {
        private const long EMPTY_ITEM_ID = -1;
    
        [SerializeField] private AnimatedGifPlayer _gifPlayer;

        protected override string TitleDisplayText => ContextData.DisplayName;

        protected override void OnInitialized()
        {
            _gifPlayer.OnReady -= DisplayGif;

            _gifPlayer.StopAllCoroutines();
            _gifPlayer.AutoPlay = true;
        
            if (!IsSameItem())
            {
                LastItemId = ContextData.ItemId;
                LastItemType = ContextData.GetType();
        
                _gifPlayer.Path = GifPath.PersistentDataPath;

                if (_gifPlayer.State == GifPlayerState.Playing)
                {
                    _gifPlayer.Pause();
                }
            
                CleanupRawImage();
                DownloadThumbnail();
            }
        
            base.OnInitialized();
        }

        protected override void OnThumbnailLoaded(long id, object obj)
        {
            if (IsDestroyed || !gameObject.activeInHierarchy || ContextData.ThumbnailOwner.Id != id) return;
        
            _gifPlayer.GifBytes = (byte[])obj;
        
            _gifPlayer.OnReady -= DisplayGif;
            _gifPlayer.OnReady += DisplayGif;
            _gifPlayer.Init();
        }
    
        protected override void OnEnable()
        {
            base.OnEnable();

            if (_gifPlayer.State == GifPlayerState.Stopped)
            {
                _gifPlayer.Play();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _gifPlayer.OnReady -= DisplayGif;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            LastItemId = EMPTY_ITEM_ID;
            LastItemType = null;
            
            if (_gifPlayer.State == GifPlayerState.Playing) _gifPlayer.Pause();
        }

        private void DisplayGif()
        {
            _gifPlayer.OnReady -= DisplayGif;

            if (!gameObject.activeInHierarchy) return;

            CoroutineSource.Instance.ExecuteWithFrameDelay(ShowRawImage);
        }
    }
}
