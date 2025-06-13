using System;
using System.Collections.Generic;
using Abstract;
using Bridge;
using Modules.Amplitude;
using UIManaging.SnackBarSystem;
using Zenject;

namespace UIManaging.Common.Buttons
{
    public class ShareVideoUrlButton: BaseContextDataButton<ShareVideoUrlButtonArgs>
    {
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private AmplitudeManager _amplitudeManager;
        
        private readonly NativeShare _nativeShare = new NativeShare();

        public event Action ShareFailed;

        protected override async void OnUIInteracted()
        {
            base.OnUIInteracted();
            
            var sharingUrlResp = await _bridge.GetSharingUlr(ContextData.VideoId);
            if (sharingUrlResp.IsError)
            {
                _snackBarHelper.ShowInformationDarkSnackBar("Video was deleted or set to private. Can't copy link");
                ShareFailed?.Invoke();
                return;
            }
            
            var shareMetaData = new Dictionary<string, object>(1)
            {
                [AmplitudeEventConstants.EventProperties.VIDEO_ID] = ContextData.VideoId
            };
            
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.SHARE_VIDEO, shareMetaData);

            _nativeShare.SetText(sharingUrlResp.Url);
            _nativeShare.Share();
        }

        protected override void OnInitialized() { }
    }
}