using TMPro;
using UIManaging.Pages.Home;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class OnboardingSeasonLikesSnackBar : SnackBar<OnboardingSeasonLikesSnackBarConfiguration>
    {
        //---------------------------------------------------------------------
        // SnackBar
        //---------------------------------------------------------------------
        public override SnackBarType Type => SnackBarType.OnboardingSeasonLikes;
        
        protected override void OnConfigure(OnboardingSeasonLikesSnackBarConfiguration configuration)
        {
            
        }
    }
}