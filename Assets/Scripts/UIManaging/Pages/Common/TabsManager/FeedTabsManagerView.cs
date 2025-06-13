using System.Threading.Tasks;
using UIManaging.Animated.Behaviours;
using UnityEngine;

namespace UIManaging.Pages.Common.TabsManager
{
    public class FeedTabsManagerView : TabsManagerView
    {
        [SerializeField] private AnimatedTabUnderlineBehaviour _animatedTabUnderline;
        
        protected override async void OnTabsSpawned()
        {
            base.OnTabsSpawned();
            await Task.Yield();
            await Task.Yield();
            _animatedTabUnderline.Initialize();
            _animatedTabUnderline.SetTargetTabIndexImmediate(TabsManagerArgs.SelectedTabIndex);
        }

        protected override void OnToggleSetOn(int index, bool setByUser)
        {
            base.OnToggleSetOn(index, setByUser);
            
            if (!_animatedTabUnderline.IsInitialized) return;

            if (setByUser)
            {
                _animatedTabUnderline.SetTargetTabIndex(index);
                _animatedTabUnderline.Play(null);
                return;
            }

            _animatedTabUnderline.SetTargetTabIndexImmediate(index);
        }
    }
}