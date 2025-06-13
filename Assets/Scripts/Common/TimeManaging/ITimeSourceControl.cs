namespace Common.TimeManaging
{
    public interface ITimeSourceControl: ITimeSource
    {
        void SetElapsed(float seconds);
        void Reset();
    }
}