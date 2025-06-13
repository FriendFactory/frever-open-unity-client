using UI.UIAnimators;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.VideoStreaming.Feed.Metrics
{
    public class ToggleAnimator : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private SequentialUiAnimationPlayer onSetAnimationPlayer;
        [SerializeField] private SequentialUiAnimationPlayer onUnsetAnimationPlayer;
    
        private void OnEnable()
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool value)
        {
            if (value)
            {
                onSetAnimationPlayer?.PlayShowAnimation();
            }
            else
            {
                onUnsetAnimationPlayer?.PlayShowAnimation();   
            }
        }
    }
}
