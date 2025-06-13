using System.Threading;

namespace Extensions
{
    public static class CancellationTokenExtensions
    {
        public static void CancelAndDispose(this CancellationTokenSource tokenSource)
        {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
        }
    }
}
