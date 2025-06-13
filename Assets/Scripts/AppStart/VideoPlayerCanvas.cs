using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace AppStart
{
    public sealed class VideoPlayerCanvas: MonoBehaviour
    {
        private const int VIDEO_WIDTH = 608;
        private const int VIDEO_HEIGHT = 1080;
        private const int VIDEO_COLOR_DEPTH = 8;
        
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private GameObject _loadingCircle;

        private RenderTexture _renderTexture;

        private Coroutine _alphaChangingCoroutine;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsVideoReady { get; private set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action VideoReady;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _renderTexture = RenderTexture.GetTemporary(VIDEO_WIDTH, VIDEO_HEIGHT, VIDEO_COLOR_DEPTH);
            _rawImage.texture = _renderTexture;
            _videoPlayer.targetTexture = _renderTexture;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            RenderTexture.ReleaseTemporary(_renderTexture);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Prepare()
        {
            _videoPlayer.prepareCompleted += OnVideoReady;
            _videoPlayer.Prepare();
        }

        public void Play()
        {
            _videoPlayer.Play();
        }

        public void Pause()
        {
            _videoPlayer.Pause();
        }
        
        public void SetTransparency(float val)
        {
            _rawImage.color = new Color(1, 1, 1, val);
        }

        public void SetTransparency(float val, float time)
        {
            if (_alphaChangingCoroutine != null)
            {
                StopCoroutine(_alphaChangingCoroutine);
            }

            //not using DoTween to exclude any chance for do tween initialization
            _alphaChangingCoroutine = StartCoroutine(ChangeTransparencySmoothly(val, time));
        }

        public void ShowLoadingIndicator()
        {
            _loadingCircle.SetActive(true);
        }
        
        public void HideLoadingIndicator()
        {
            _loadingCircle.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnVideoReady(VideoPlayer videoPlayer)
        {
            _videoPlayer.prepareCompleted -= OnVideoReady;
            IsVideoReady = true;
            VideoReady?.Invoke();
        }

        private IEnumerator ChangeTransparencySmoothly(float endValue, float duration)
        {
            var startAlpha = _rawImage.color.a;
            var timer = 0f;
            while (timer < duration)
            {
                yield return null;
                timer += Time.deltaTime;
                var nextAlpha = Mathf.Lerp(startAlpha, endValue, timer / duration);
                _rawImage.color = new Color(1, 1, 1, nextAlpha);
            }
        }
    }
}