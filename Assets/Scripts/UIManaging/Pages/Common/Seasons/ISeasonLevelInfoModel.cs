using UIManaging.Animated.Behaviours;

namespace UIManaging.Pages.Common.Seasons
{
    public interface ISeasonLevelInfoModel: IAnimatedSliderModel
    {
        int Level { get; }

        bool UpdateForNextCycle();
    }
}