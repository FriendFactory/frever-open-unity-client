using System;
using System.Threading;
using System.Threading.Tasks;

namespace Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Runs TaskDelay API inside try-catch block
        /// </summary>
        public static async Task DelayWithoutThrowingCancellingException(int delayMs, CancellationToken token)
        {
            try
            {
                await Task.Delay(delayMs, token);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}