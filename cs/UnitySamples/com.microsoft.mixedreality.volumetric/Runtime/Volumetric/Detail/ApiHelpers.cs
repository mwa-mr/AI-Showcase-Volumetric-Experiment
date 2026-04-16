// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VolumetricCsInternalLibrary")]

namespace Microsoft.MixedReality.Volumetric.Detail
{
    internal sealed partial class Api
    {
        internal sealed class ApiException : Exception
        {
            public Api.VaResult Result { get; private set; }
            public ApiException(Api.VaResult _result, string? _message) : base(_message)
            {
                Result = _result;
            }
        }

        internal sealed class LibraryException : Exception
        {
            public readonly string FileName;
            public readonly int LineNumber;

            public LibraryException(string? message,
                                    [CallerFilePath] string fileName = "",
                                    [CallerLineNumber] int lineNumber = 0)
                : base($"{message} (File: {fileName}, Line: {lineNumber})")
            {
                FileName = fileName;
                LineNumber = lineNumber;
            }
        }

        internal static bool VaSucceeded(VaResult result)
        {
            return result >= 0;
        }

        internal static bool VaFailed(VaResult result)
        {
            return result < 0;
        }

        internal static Api.VaResult CheckResult(Api.VaResult result,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (VaFailed(result))
            {
                string msg = $"VaResult error:{result}, in {memberName}() at {filePath}:{lineNumber}";
                Trace.LogError(() => msg);
                throw new ApiException(result, msg);
            }
            return result;
        }

        internal static T VaEventConvert<T>(Api.VaEventDataBuffer buffer) where T : struct
        {
            T dest;
            int sizeSrc = Marshal.SizeOf<Api.VaEventDataBuffer>();
            IntPtr ptrSrc = IntPtr.Zero;
            try
            {
                // allocate unmanaged memory and copy the event buffer
                ptrSrc = Marshal.AllocHGlobal(sizeSrc);
                Marshal.StructureToPtr(buffer, ptrSrc, false);

                // marshals the data from unmanaged memory to a new T managed object
                object? temp = Marshal.PtrToStructure<T>(ptrSrc);
                if (temp is null)
                {
                    throw new ArgumentException("Invalid VaEventDataBuffer");
                }
                dest = (T)temp;
            }
            finally
            {
                Marshal.FreeHGlobal(ptrSrc);
            }

            return dest;
        }

        internal static T ToDelegate<T>(IntPtr functionPointer)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(functionPointer, typeof(T));
        }

        internal static T? GetFunctionPointer<T>(IntPtr session, string functionName) where T : Delegate
        {
            IntPtr pfn = IntPtr.Zero;
            var result = vaGetFunctionPointer?.Invoke(session, functionName, out pfn);
            if (result < 0 && pfn == IntPtr.Zero)
            {
                Trace.LogWarning(() => $"GetFunctionPointer failed with function name {functionName}");
                return null;
            }
            return ToDelegate<T>(pfn);
        }
    }

    internal static class ApiExtensions
    {
        internal static void IfHasValue<T>(this T? nullable, Action<T> action) where T : class
        {
            if (nullable is not null)
            {
                action(nullable!);
            }
        }

        internal static void IfHasValue<T>(this T? nullable, Action<T> action) where T : struct
        {
            if (nullable.HasValue)
            {
                action(nullable.Value);
            }
        }
    }
}
