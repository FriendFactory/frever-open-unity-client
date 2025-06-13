using UIManaging.Common.Skeletons;
using UnityEngine;

namespace UIManaging.Common.Args.Views.Profile
{
    public sealed class UserPortraitWithSkeleton: UserPortrait
    {
        [SerializeField] private UISkeleton _skeleton;

        protected override void OnActivated()
        {
            base.OnActivated();

            if (!_skeleton.IsDisplayed)
            {
                _skeleton.Show();
            }
        }

        protected override void OnShowContent()
        {
            base.OnShowContent();
            
            _skeleton.Hide();
        }
    }
}