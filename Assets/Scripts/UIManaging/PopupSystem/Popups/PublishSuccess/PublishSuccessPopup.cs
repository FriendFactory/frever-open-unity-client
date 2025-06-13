using Bridge;
using BrunoMikoski.AnimationSequencer;
using Common.Publishers;
using JetBrains.Annotations;
using Modules.VideoSharing;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.PopupSystem.Popups.PublishSuccess.Navigation;
using UIManaging.PopupSystem.Popups.PublishSuccess.VideoSharing;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    public class PublishSuccessPopup: BasePopup<PublishSuccessPopupConfiguration>
    {
        [SerializeField] private PublishSuccessView _publishSuccessView;
        [Header("Animations")] 
        [SerializeField] private AnimationSequencerController _animationSequencer;
        [SerializeField] private AnimationSequencerController _fadeOutCanvasGroupSequencer;

        private IBridge _bridge;
        
        private PublishSuccessPresenter _publishSuccessPresenter;
        private VideoSharingPresenter _videoSharingPresenter; 
        private PublishSuccessNavigationFlowHandler _navigationFlowHandler;

        [Inject, UsedImplicitly]
        private void Construct(PopupManager popupManager, VideoSharingHelper videoSharingHelper, PageManager pageManager,
            LocalUserDataHolder localUserDataHolder, VideoManager videoManager, IBridge bridge, SnackBarHelper snackBarHelper, IPublishVideoHelper publishVideoHelper)
        {
            _navigationFlowHandler = new PublishSuccessNavigationFlowHandler(popupManager, pageManager, videoManager, bridge, snackBarHelper, publishVideoHelper);
            _publishSuccessPresenter = new PublishSuccessPresenter(videoSharingHelper, bridge);
        }

        protected override void OnConfigure(PublishSuccessPopupConfiguration configuration)
        {
                _navigationFlowHandler.Initialize();

                var publishSuccessModel = new PublishSuccessModel(configuration.Video, configuration.CreatorScoreModel,
                                                                  configuration.VideoSharingModel,
                                                                  configuration.GenerateTemplateWithName);

                _publishSuccessPresenter.Initialize(publishSuccessModel, _publishSuccessView);
                _publishSuccessView.Initialize(publishSuccessModel);
        }
        
        public override void Show()
        {
            base.Show();
            
            _animationSequencer.Rewind();
            _animationSequencer.PlayForward();
        }

        protected override void OnHidden()
        {
            _navigationFlowHandler.Dispose();

            _publishSuccessPresenter.CleanUp();
            _publishSuccessView.CleanUp();

            _animationSequencer.ResetToInitialState();
            _animationSequencer.Kill();
        }

        public override void Hide()
        {
            _fadeOutCanvasGroupSequencer.Play();
            _animationSequencer.PlayBackwards(true, () => { base.Hide(null); });
        }

        public override void Hide(object result)
        {
            _fadeOutCanvasGroupSequencer.Play();
            _animationSequencer.PlayBackwards(true, () => { base.Hide(result); });
        }
    }
}