namespace Common.TimeManaging
{
    /// <summary>
    ///     Shared between all asset recorders which requires event time
    /// </summary>
    public interface ITimeSource
    {
        long ElapsedMs { get; }
        float ElapsedSeconds { get; }
    }
}