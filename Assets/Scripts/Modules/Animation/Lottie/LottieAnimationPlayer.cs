using System.Collections;
using LottiePlugin;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Modules.Animation.Lottie
{
    [RequireComponent(typeof(RawImage))]
    public class LottieAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private TextAsset _animationJson;
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private float _animationSpeed = 1f;
        [SerializeField] private uint _textureWidth;
        [SerializeField] private uint _textureHeight;
        [FormerlySerializedAs("_playOnAwake")] [SerializeField] private bool _playOnStart = true;
        [SerializeField] private bool _loop = true;

        public Transform Transform { get; private set; }

        public RawImage RawImage
        {
            get => _rawImage;
            internal set => _rawImage = value;
        }

        public TextAsset AnimationJson => _animationJson;
        public uint TextureWidth => _textureWidth;
        public uint TextureHeight => _textureHeight;
        public LottieAnimation LottieAnimation => _lottieAnimation;
        public float AnimationSpeed => _animationSpeed;
        public bool Loop => _loop;

        private LottieAnimation _lottieAnimation;
        private Coroutine _renderLottieAnimationCoroutine;
        private WaitForEndOfFrame _waitForEndOfFrame;

        private void Awake()
        {
            Transform = transform;
            _waitForEndOfFrame = new WaitForEndOfFrame();
        }

        private void Start()
        {
            if (_animationJson == null)
            {
                return;
            }

            if (_rawImage == null)
            {
                _rawImage = GetComponent<RawImage>();
            }

            _lottieAnimation = CreateIfNeededAndReturnLottieAnimation();
            
            if (_playOnStart)
            {
                Play();
            }
            else
            {
                _lottieAnimation.DrawOneFrame(0);
            }
        }

        private void OnDestroy()
        {
            DisposeLottieAnimation();
        }

        [ContextMenu("Play")]
        public void Play()
        {
            if (_renderLottieAnimationCoroutine != null)
            {
                StopCoroutine(_renderLottieAnimationCoroutine);
            }
            
            if (!Application.isPlaying) return;

            _lottieAnimation ??= CreateIfNeededAndReturnLottieAnimation();
            
            _lottieAnimation.Play();
            _renderLottieAnimationCoroutine = StartCoroutine(RenderLottieAnimationCoroutine());
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            if (_renderLottieAnimationCoroutine != null)
            {
                StopCoroutine(_renderLottieAnimationCoroutine);
                _renderLottieAnimationCoroutine = null;
            }

            _lottieAnimation.Stop();
        }

        public void Rewind()
        {
            Stop();
            _lottieAnimation.DrawOneFrame(0);
        }

        internal LottieAnimation CreateIfNeededAndReturnLottieAnimation()
        {
            if (_animationJson == null)
            {
                return null;
            }

            if (_rawImage == null)
            {
                _rawImage = GetComponent<RawImage>();
            }

            if (_rawImage == null)
            {
                return null;
            }

            if (_lottieAnimation == null)
            {
                _lottieAnimation = LottieAnimation.LoadFromJsonData(
                    _animationJson.text,
                    string.Empty,
                    _textureWidth,
                    _textureHeight);
                _rawImage.texture = _lottieAnimation.Texture;
            }

            return _lottieAnimation;
        }

        internal void DisposeLottieAnimation()
        {
            if (_lottieAnimation != null)
            {
                _lottieAnimation.Dispose();
                _lottieAnimation = null;
            }
        }

        private IEnumerator RenderLottieAnimationCoroutine()
        {
            while (true)
            {
                yield return _waitForEndOfFrame;
                if (_lottieAnimation != null)
                {
                    _lottieAnimation.Update(_animationSpeed);
                    if (!_loop && _lottieAnimation.CurrentFrame == _lottieAnimation.TotalFramesCount - 1)
                    {
                        Stop();
                    }
                }
            }
        }
    }
}