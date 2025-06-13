using TipsManagment;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.QuestManaging.Redirections
{
    public class JoinStyleChallengeTip : OnboardingTip
    {
        private const float IPHONE_14_SCREEN_RATIO = 0.47f;
        
        [SerializeField] private LayoutElement _cutout;
        
        protected override void PositionateTip()
        {
            const float aspect9to16 = 0.56f;
            var aspect = (float)Screen.width / Screen.height;
            var is9to16 = Mathf.Abs(aspect - aspect9to16) < 0.01f;
            if (is9to16)
            {
                _cutout.preferredHeight = 180;
            }
        }
        
    }
}