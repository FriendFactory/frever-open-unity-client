using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace TipsManagment
{
    public class TimedOnboardingTip: OnboardingTip
    {
        [SerializeField] private float _disappearTimer = 4f;
        
        public override async Task Activate()
        {
            await base.Activate();

            StartCoroutine(DisappearTimer(_disappearTimer));
        }

        private IEnumerator DisappearTimer(float disappearTimer)
        {
            yield return new WaitForSeconds(disappearTimer);
            
            CompleteTip();
        }
    }
}