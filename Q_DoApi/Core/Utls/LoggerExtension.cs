using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Q_DoApi.Core.Utls
{
    internal static class LoggerExtension
    {
        public static void Event(this ILogger? logger, string message, [CallerMemberName] string? method = null)
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
