// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

using System;

namespace Microsoft.MixedReality.Volumetric
{
    /// <summary>
    /// Provides tracing/logging functionality for the Volumetric SDK.
    /// </summary>
    public static class VaTrace
    {
        /// <summary>
        /// Enables or disables tracing output to the console.
        /// Disabled by default. Intended for debugging only.
        /// Has no effect if no console output stream is available.
        /// </summary>
        public static bool EnableTraceToConsole
        {
            get => Detail.Trace.EnableTraceToConsole;
            set => Detail.Trace.EnableTraceToConsole = value;
        }

        /// <summary>
        /// Logs an informational message to the application's trace log.
        /// </summary>
        /// <param name="message">The informational message to be logged.</param>
        public static void LogInfo(Func<string> message)
        {
            Detail.Trace.LogInfo(message);
        }


        /// <summary>
        /// Logs a warning message to the application's trace log.
        /// </summary>
        /// <param name="message">The warning message to be logged.</param>
        public static void LogWarning(Func<string> message)
        {
            Detail.Trace.LogWarning(message);
        }

        /// <summary>
        /// Logs an error message to the application's trace log.
        /// </summary>
        /// <param name="message">The error message to be logged.</param>
        public static void LogError(Func<string> message)
        {
            Detail.Trace.LogError(message);
        }
    }


    /// <summary>
    /// A helper class for scoped tracing with start and stop events.
    /// Usage:
    /// using var _ = new VaScopedTrace("MyMessage");
    /// </summary>
    public sealed class VaScopedTrace : IDisposable
    {
        private readonly Func<string> message;

        /// <summary>
        /// Starts a scoped trace with the specified message.
        /// </summary>
        /// <param name="message"></param>
        public VaScopedTrace(Func<string> message)
        {
            this.message = message;
            Detail.Trace.LogStartEvent(message);
        }

        /// <summary>
        /// Disposes the scoped trace and logs the stop event.
        /// </summary>
        public void Dispose()
        {
            Detail.Trace.LogStopEvent(message);
        }
    }
}
