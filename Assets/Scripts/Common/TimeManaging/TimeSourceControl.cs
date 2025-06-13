namespace Common.TimeManaging
{
    public class TimeSourceControl: ITimeSourceControl
    {
        public long ElapsedMs => (long)(ElapsedSeconds * 1000);
        public float ElapsedSeconds { get; private set; }
        
        public void SetElapsed(float seconds)
        {
            ElapsedSeconds = seconds;
        }

        public void Reset()
        {
            SetElapsed(0);
        }
    }
}