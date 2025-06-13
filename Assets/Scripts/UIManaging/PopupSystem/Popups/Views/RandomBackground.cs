using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Extensions;
using Sirenix.OdinInspector;
using UIManaging.Animated.Behaviours;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace UIManaging.PopupSystem.Popups.Views
{
    public class RandomBackground : MonoBehaviour
    {
        private const int PROD_BACKGROUND_COUNT = 30;
        private const int TEST_ENVS_BACKGROUND_COUNT = 3;
        private const string FILE_NAME = "loading_background_{0}";
        
        private const int RANDOM_CACHE_SIZE = 3;

        [SerializeField] private float _timeBetweenSwitches;
        [SerializeField] private FadeInOutBehaviour _fadeBehaviour;

        [SerializeField] private RawImage _firstImage;
        [SerializeField] private AspectRatioFitter _firstImageFitter;
        [SerializeField] private RawImage _secondImage;
        [SerializeField] private AspectRatioFitter _secondImageFitter;

        [Inject] private IBridge _bridge;

        private readonly Queue _lastRolls = new Queue(RANDOM_CACHE_SIZE);
        private Coroutine _coroutine;
        private WaitForSeconds _waitForSeconds;
        private CancellationTokenSource _cancellationTokenSource;

        private static bool _prefetching;
        private static readonly List<int> _fetchedImageIds = new List<int>();
        private static Texture _lastUsedTexture;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var index = Random.Range(0, _fetchedImageIds.Count);

            var key = string.Format(FILE_NAME, _fetchedImageIds[index]);
            var result = _bridge.GetImageFromCache(key);
            _lastUsedTexture = result.Model;

            UpdateTexture(_firstImage, _firstImageFitter, _lastUsedTexture);
            UpdateTexture(_secondImage, _secondImageFitter, _lastUsedTexture);
        }

        private void OnDisable()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.CancelAndDispose();
                _cancellationTokenSource = null;
            }
            if(_coroutine == null) return;
            
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static async void PrefetchImages(IBridge bridge)
        {
            await PrefetchImagesAsync(bridge);
        }

        [Button]
        public void Play(CancellationToken token)
        {
            _waitForSeconds = new WaitForSeconds(_timeBetweenSwitches);
            _coroutine = StartCoroutine(BackgroundRoutine(token));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IEnumerator BackgroundRoutine(CancellationToken token)
        {
            while (true)
            {
                yield return _waitForSeconds;

                var index = Roll();
                if (token.IsCancellationRequested) break;

                SwapImages(index);
            }
        }

        private int Roll()
        {
            var availableImages = _fetchedImageIds.Where(x => !_lastRolls.Contains(x));
            var index = Random.Range(0, availableImages.Count());

            ManageCache();

            _lastRolls.Enqueue(index);

            return index;
        }

        private void SwapImages(int index)
        {
            var key = string.Format(FILE_NAME, index);
            var newBackgroundResult = _bridge.GetImageFromCache(key);
            
            if (newBackgroundResult.IsError) Debug.LogError(newBackgroundResult.ErrorMessage);

            UpdateTexture(_firstImage, _firstImageFitter, _secondImage.texture);
            var newBackground = newBackgroundResult.Model;
            UpdateTexture(_secondImage, _secondImageFitter, newBackground);
            _lastUsedTexture = newBackground;
            
            _fadeBehaviour.FadeOut();
        }

        private void ManageCache()
        {
            var cacheSize = _fetchedImageIds.Count < RANDOM_CACHE_SIZE ? 1 : RANDOM_CACHE_SIZE;
            if (_lastRolls.Count < cacheSize) return;
            
            _lastRolls.Dequeue();
            ManageCache();
        }

        private static async Task PrefetchImagesAsync(IBridge bridge)
        {
            if(_prefetching || _fetchedImageIds.Count != 0) return;

            _prefetching = true;
        
            var availableImages = bridge.Environment == FFEnvironment.Production ? PROD_BACKGROUND_COUNT : TEST_ENVS_BACKGROUND_COUNT;
            for (var i = 0; i < availableImages; i++)
            {
                var key = string.Format(FILE_NAME, i);

                var result = await bridge.FetchImageAsync(key);
                if (result.IsSuccess)
                {
                    if (_lastUsedTexture == null)
                    {
                        var imageResult = await bridge.GetImageAsync(key);
                        if (imageResult.IsSuccess) _lastUsedTexture = imageResult.Model;
                    }
                    
                    _fetchedImageIds.Add(i);
                    continue;
                }
                
                Debug.LogWarning($"Couldn't fetch {key}");
            }

            _prefetching = false;
        }

        private void UpdateTexture(RawImage image, AspectRatioFitter fitter, Texture texture)
        {
            image.texture = texture;
            image.color = Color.white;
            fitter.aspectRatio = (float) texture.width / texture.height;
        }
    }
}