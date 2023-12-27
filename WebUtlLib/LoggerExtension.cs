using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace WebUtlLib
{
    public static class LoggerExtension
    {
        public static void Event(this ILogger? logger, [CallerMemberName] string? method = null) =>
            logger?.EventLog("Invoke!", method);

        public static void EventLog(this ILogger? logger, string? message,[CallerMemberName]string? method = null)
        {
            var formattedMessage = $"{method}(): {message}";
            logger?.LogInformation(formattedMessage);
        }

        public static void Event<T>(this ILogger? logger, T type, string message,
            [CallerMemberName] string? method = null) where T : class
        {
            var formattedMessage = $"{type?.GetType().Name}.{method}(): {message}";
            logger?.LogInformation(formattedMessage);
        }
    }
}