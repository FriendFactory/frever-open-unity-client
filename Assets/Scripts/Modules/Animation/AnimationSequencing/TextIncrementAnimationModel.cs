namespace Modules.AnimationSequencing
{
    public class TextIncrementAnimationModel
    {
        public int From { get; }
        public int To { get; }

        public TextIncrementAnimationModel(int  from, int to)
        {
            From = from;
            To = to;
        }
    }
}