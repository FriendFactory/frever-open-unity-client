using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class VideoSharedToChatSnackbar : SnackBar<VideoSharedToChatSnackbarConfiguration>
    {
        [SerializeField] private Button _viewButton;

        private void Awake()
        {
            _viewButton.onClick.AddListener(OnViewButton);
        }

        private void OnViewButton()
        {
            Configuration.OnViewButton?.Invoke();
        }

        public override SnackBarType Type => SnackBarType.VideoSharedToChat;
        protected override void OnConfigure(VideoSharedToChatSnackbarConfiguration configuration)
        {
            
        }
    }
}