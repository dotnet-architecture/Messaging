// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.Internal
{
    internal static class HostingLoggerExtensions
    {
        public static void ApplicationError(this ILogger logger, EventId eventId, string message, Exception exception)
        {
            var reflectionTypeLoadException = exception as ReflectionTypeLoadException;
            if (reflectionTypeLoadException != null)
            {
                foreach (var ex in reflectionTypeLoadException.LoaderExceptions)
                {
                    message = message + Environment.NewLine + ex.Message;
                }
            }

            logger.LogCritical(
                eventId: eventId,
                message: message,
                exception: exception);
        }

        public static void Starting(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                   eventId: LoggerEventIds.Starting,
                   message: "Hosting starting");
            }
        }

        public static void Started(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.Started,
                    message: "Hosting started");
            }
        }

        public static void Shutdown(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.Shutdown,
                    message: "Hosting shutdown");
            }
        }

        public static void ServerShutdownException(this ILogger logger, Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.ServerShutdownException,
                    exception: ex,
                    message: "Server shutdown exception");
            }
        }
    }
}
