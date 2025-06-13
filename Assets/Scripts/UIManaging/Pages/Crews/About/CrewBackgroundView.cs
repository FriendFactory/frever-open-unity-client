using System.Threading;
using Abstract;
using Bridge;
using Bridge.Models.Common;
using Extensions;
using UIManaging.Animated.Behaviours;
using UIManaging.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewBackgroundView : BaseContextDataView<IThumbnailOwner>
    {
        [SerializeField] private AnimatedSkeletonBehaviour _skeletonBehaviour;
        [SerializeField] private RawImage _background;
        [SerializeField] private ThumbnailLoader _thumbnailLoader;

        [Inject] private IBridge _bridge;

        private CancellationTokenSource _tokenSource;

        private void OnEnable()
        {
            _skeletonBehaviour.Play();
            _thumbnailLoader.OnThumbnailReady += HideSkeleton;
            
            if (ContextData is null) return;

            _tokenSource = new CancellationTokenSource();
            _thumbnailLoader.Initialize(ContextData);
        }

        private void OnDisable()
        {
            _tokenSource.CancelAndDispose();
            _thumbnailLoader.OnThumbnailReady -= HideSkeleton;
        }

        public void CancelLoading()
        {
            _thumbnailLoader.CancelThumbnailLoading();
        }
        
        public void Refresh()
        {
            _thumbnailLoader.CancelThumbnailLoading();
            _thumbnailLoader.Initialize(ContextData);
        }

        protected override void OnInitialized()
        {
            if (!gameObject.activeSelf) return;
            Refresh();
        }

        private void HideSkeleton()
        {
            _skeletonBehaviour.FadeOut();
            _skeletonBehaviour.SetActive(false);
        }
    }
}