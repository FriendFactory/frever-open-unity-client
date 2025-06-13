using System;

namespace Common.Exceptions
{
    /// <summary>
    /// This exception won't be shown as error popup to the user, but will be send to analytics
    /// It helps to collect necessary data for localizing the issue, if reproduce steps are not known
    /// </summary>
    public sealed class SilentException : Exception
    {
        public SilentException(string message) : base(SilentErrorsHelper.BuildSilentErrorMessage(message))
        {
        }
        
        public SilentException(int jiraTicketNumber, string message) : base(SilentErrorsHelper.BuildSilentErrorMessage(jiraTicketNumber, message))
        {
        }
    }

    public static class DebugHelper
    {
        /// <summary>
        /// Error popup won't be shown for user(except QA), but error will be sent to the analytics
        /// It helps to collect necessary data for localizing the issue, if reproduce steps are not known
        /// </summary>
        public static void LogSilentError(string message)
        {
            var logMessage = SilentErrorsHelper.BuildSilentErrorMessage(message);
            Debug.LogError(logMessage);
        }
        
        /// <summary>
        /// Error popup won't be shown for user(except QA), but error will be sent to the analytics
        /// It helps to collect necessary data for localizing the issue, if reproduce steps are not known
        /// </summary>
        public static void LogSilentError(int jiraTicketNumber, string message)
        {
            var logMessage = SilentErrorsHelper.BuildSilentErrorMessage(jiraTicketNumber, message);
            Debug.LogError(logMessage);
        }
    }

    internal static class SilentErrorsHelper
    {
        public static string BuildSilentErrorMessage(string message)
        {
            return $"{ErrorConstants.IGNORED_MESSAGE_PREFIX}:{message}";
        }
        
        public static string BuildSilentErrorMessage(int jiraTicketNumber, string message)
        {
            return $"{ErrorConstants.IGNORED_MESSAGE_PREFIX}:FREV-{jiraTicketNumber}:{message}";
        }
    }

    public class ErrorConstants
    {
        public const string IGNORED_MESSAGE_PREFIX = "FREVER_SILENT_EXC";
        public const string LOW_DISK_SPACE_ERROR_MESSAGE = "Please free up the disk on your device";
    }
}
