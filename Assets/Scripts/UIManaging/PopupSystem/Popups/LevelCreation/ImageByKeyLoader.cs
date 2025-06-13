using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Results;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.LevelCreation
{
    /// <summary>
    /// Loads image from the cloud bucket by key
    /// </summary>
    public sealed class ImageByKeyLoader: MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private bool _preserveAspect = true;
        
        [Inject] private IBridge _bridge;

        private CancellationTokenSource _tokenSource;

        public event Action Loaded;
        
        private CancellationTokenSource TokenSource
        {
            get { return _tokenSource ??= new CancellationTokenSource(); }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            TokenSource?.Cancel();
            TokenSource?.Dispose();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void LoadImageAsync(string key)
        {
            _image.sprite = await GetSprite(key, TokenSource.Token);

            _image.preserveAspect = _preserveAspect;
            _image.type = Image.Type.Simple;
            Loaded?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task<Sprite> GetSprite(string key, CancellationToken token)
        {
            Result<Texture2D> resp;
            if (_bridge.HasImageCached(key))
            {
                resp = _bridge.GetImageFromCache(key);
            }
            else
            {
                resp = await _bridge.GetImageAsync(key, true, token);
            }
            
            return !resp.IsSuccess ? null : resp.Model.ToSprite();
        }
    }
}