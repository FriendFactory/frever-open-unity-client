using UI.UIAnimators;
using UnityEngine;

namespace Modules.VideoStreaming.Feed
{
    public class FeedVideoTabAnimator : MonoBehaviour
    {
        [SerializeField] private BaseUiAnimationPlayer uiAnimationPlayer;

        private bool _lastToggleValue;

        public void PlayShowAnimation(bool instant = false)
        {
            if(instant)
            {
                uiAnimationPlayer.PlayShowAnimation();
            }
            else
            {
                uiAnimationPlayer.PlayShowAnimationInstant();
            }
        }
        
        public void PlayHideAnimation(bool instant = false)
        {
            if(instant)
            {
                uiAnimationPlayer.PlayHideAnimation();
            }
            else
            {
                uiAnimationPlayer.PlayHideAnimationInstant();
            }
        }
    }
}