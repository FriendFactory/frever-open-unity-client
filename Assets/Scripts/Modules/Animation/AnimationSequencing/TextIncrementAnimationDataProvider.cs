using Common.Abstract;

namespace Modules.AnimationSequencing
{
    public abstract class TextIncrementAnimationDataProvider: BaseContextPanel<TextIncrementAnimationModel>
    {
        public TextIncrementAnimationModel AnimationModel => ContextData;

        protected override void OnInitialized() { }
    }
}