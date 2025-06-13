using System.Threading.Tasks;
using UnityEngine;

namespace TipsManagment
{
    public class OnboardingTip: BaseTip
    {
        public override Task Activate()
        {
            TargetTransform = Args.PageTransform as RectTransform;
            StartTip();
            return Task.CompletedTask;
        }

        public virtual void CompleteTip()
        {
            TaskSource?.TrySetResult(true);
        }
        
        protected override void PositionateTip()
        {
        }
    }
}