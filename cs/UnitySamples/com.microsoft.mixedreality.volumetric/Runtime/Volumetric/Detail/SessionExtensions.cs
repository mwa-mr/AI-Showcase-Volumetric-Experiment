// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    using static Detail.Api;

    internal sealed class SessionExtensions
    {
        private Dictionary<string, uint> _enabledExtensions = new();
        public IReadOnlyDictionary<string, uint> EnabledExtensions => _enabledExtensions;

        private List<string> _missingRequiredExtensions = new();
        public IReadOnlyList<string> MissingRequiredExtensions => _missingRequiredExtensions;

        internal bool IsEnabled(string extension)
        {
            return _enabledExtensions.ContainsKey(extension);
        }

        internal void Initialize(PFN_vaGetFunctionPointer getFunctionPointer, string[] requiredExtensions, string[] optionalExtensions)
        {
            _enabledExtensions.Clear();
            _missingRequiredExtensions = requiredExtensions.ToList();

            IntPtr function;
            CheckResult(getFunctionPointer(IntPtr.Zero, "vaEnumerateExtensions", out function));
            PFN_vaEnumerateExtensions vaEnumerateExtensions = ToDelegate<PFN_vaEnumerateExtensions>(function);

            uint dataCount = 0;
            CheckResult(vaEnumerateExtensions(0, out dataCount, IntPtr.Zero));
            Trace.LogInfo(() => $"vaEnumerateExtensions: Count = {dataCount}");

            if (dataCount == 0)
            {
                return;
            }

            IntPtr dataArray = Marshal.AllocHGlobal(Marshal.SizeOf<VaExtensionProperties>() * (int)dataCount);
            try
            {
                var itemSize = Marshal.SizeOf<VaExtensionProperties>();
                VaExtensionProperties prototype = new() { type = Api.VaStructureType.VA_TYPE_EXTENSION_PROPERTIES };
                for (int i = 0; i < dataCount; i++)
                {
                    IntPtr itemPtr = dataArray + i * itemSize;
                    Marshal.StructureToPtr(prototype, itemPtr, false);
                }

                CheckResult(vaEnumerateExtensions(dataCount, out dataCount, dataArray));

                for (int i = 0; i < dataCount; i++)
                {
                    IntPtr itemPtr = dataArray + i * itemSize;
                    VaExtensionProperties props = Marshal.PtrToStructure<VaExtensionProperties>(itemPtr);

                    var extensionName = props.extensionName;

                    if (requiredExtensions.Any(s => s == extensionName))
                    {
                        Trace.LogInfo(() => $"vaEnumerateExtensions: enable required extension = {extensionName}");

                        _enabledExtensions[extensionName] = props.extensionVersion;
                        _missingRequiredExtensions.Remove(extensionName);
                    }
                    else if (optionalExtensions.Any(s => s == extensionName))
                    {
                        Trace.LogInfo(() => $"vaEnumerateExtensions: enable optional extension = {extensionName}");

                        _enabledExtensions[extensionName] = props.extensionVersion;
                    }
                    else
                    {
                        // No need to log those extensions that app didn't request
                    }
                }
            }
            catch (Exception e)
            {
                Trace.LogError(() => $"vaEnumerateExtensions: {e.Message}");
            }
            finally
            {
                Marshal.FreeHGlobal(dataArray);
            }
        }
    }
}
