// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric.Detail
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    [EventSource(Name = "Microsoft.MixedReality.Volumetric.VaSDKCS", Guid = "35d8626a-39b9-5e50-91cf-a0616d3aed3b")]
    internal sealed class VolumetricEventSource : EventSource
    {
        public static readonly VolumetricEventSource Log = new VolumetricEventSource();
        private VolumetricEventSource() { }

        private enum EventIds
        {
            Info = 1,
            Warning = 2,
            Error = 3,
            StartEvent = 4,
            StopEvent = 5
        }

        [Event((int)EventIds.Info, Level = EventLevel.Informational, Message = "Info: {0}")]
        public void Info(string message)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent((int)EventIds.Info, message);
        }
        [Event((int)EventIds.Warning, Level = EventLevel.Warning, Message = "Warning: {0}")]
        public void Warning(string message)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent((int)EventIds.Warning, message);
        }
        [Event((int)EventIds.Error, Level = EventLevel.Error, Message = "Error: {0}")]
        public void Error(string message)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent((int)EventIds.Error, message);
        }

        [Event((int)EventIds.StartEvent, Level = EventLevel.Informational, Message = "Start: {0}", Opcode = EventOpcode.Start)]
        public void StartEvent(string message)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent((int)EventIds.StartEvent, message);
        }

        [Event((int)EventIds.StopEvent, Level = EventLevel.Informational, Message = "Stop: {0}", Opcode = EventOpcode.Stop)]
        public void StopEvent(string message)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent((int)EventIds.StopEvent, message);
        }
    }

    internal static class Trace
    {
        public static bool EnableTraceToConsole { get; set; }

        private enum LogLevel
        {
            Fatal = 0,
            Error = 1,
            Warning = 2,
            Info = 3
        }

        sealed class CheckException : Exception
        {
            public CheckException(string? message) : base(message) { }
        }

        public static void Check(bool value,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!value)
            {
                string msg = $"Check error: {memberName} at [{lineNumber}] in {filePath}";
                LogError(() => msg);
                throw new CheckException(msg);
            }
        }

        public static void LogInfo(Func<string> message)
        {
            Log((int)LogLevel.Info, "Info", message);
            if (VolumetricEventSource.Log.IsEnabled())
            {
                VolumetricEventSource.Log.Info(message());
            }
        }

        public static void LogWarning(Func<string> message)
        {
            Log((int)LogLevel.Warning, "Warning", message);
            if (VolumetricEventSource.Log.IsEnabled())
            {
                VolumetricEventSource.Log.Warning(message());
            }
        }

        public static void LogError(Func<string> message)
        {
            Log((int)LogLevel.Error, "Error", message);
            if (VolumetricEventSource.Log.IsEnabled())
            {
                VolumetricEventSource.Log.Error(message());
            }
        }

        public static void LogStartEvent(Func<string> message)
        {
            Log((int)LogLevel.Info, "Start", message);
            if (VolumetricEventSource.Log.IsEnabled())
            {
                VolumetricEventSource.Log.StartEvent(message());
            }
        }

        public static void LogStopEvent(Func<string> message)
        {
            Log((int)LogLevel.Info, "Stop", message);
            if (VolumetricEventSource.Log.IsEnabled())
            {
                VolumetricEventSource.Log.StopEvent(message());
            }
        }

        private static string Now()
        {
            return DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        private static void Log(int level, string category, Func<string> message)
        {
            string formattedMessage = "";
            if (Debugger.IsLogging())
            {
                formattedMessage = string.Format(CultureInfo.InvariantCulture, "[VA:{0}][{1}] {2}", category, Now(), message());
                Debugger.Log(level, category, formattedMessage + Environment.NewLine);
            }
            if (EnableTraceToConsole && Console.Out != null)
            {
                if (formattedMessage.Length == 0)
                {
                    formattedMessage = string.Format(CultureInfo.InvariantCulture, "[VA:{0}][{1}] {2}", category, Now(), message());
                }
                Console.WriteLine(formattedMessage);
            }
        }

        public static void LogFatal(Func<string> message)
        {
            Log((int)LogLevel.Fatal, "Fatal", message);
            if (VolumetricEventSource.Log.IsEnabled())
            {
                VolumetricEventSource.Log.Error(message());
            }
        }
    }

    internal sealed class ScopedTrace : IDisposable
    {
        private readonly Func<string> message;
        public ScopedTrace(Func<string> message)
        {
            this.message = message;
            Trace.LogStartEvent(message);
        }
        public void Dispose()
        {
            Trace.LogStopEvent(message);
        }
    }
}
