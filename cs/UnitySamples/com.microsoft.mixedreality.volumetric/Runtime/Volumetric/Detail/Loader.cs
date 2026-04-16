// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric.Detail
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Text;

    using static Api;

    internal sealed class Loader
    {
        public static PFN_vaGetFunctionPointer LoadRuntime()
        {
            var pfn = IntPtr.Zero;
            Detail.Api.CheckResult(vaNegotiateRuntime(out pfn, VA_MINIMUM_VERSION, VA_MAXIMUM_VERSION));
            return ToDelegate<PFN_vaGetFunctionPointer>(pfn);
        }

        private const uint VA_LOADER_INFO_STRUCT_VERSION = 1;
        private const uint VA_CURRENT_LOADER_RUNTIME_VERSION = 1;
        private const uint VA_RUNTIME_INFO_STRUCT_VERSION = 1;

        // This loader is compatible with any runtime with major version 0, at the API preview phase.
        private static VaVersion VA_MINIMUM_VERSION = Api.VaMakeVersion(0, 2, 0);
        private static VaVersion VA_MAXIMUM_VERSION = Api.VaMakeVersion(0, 0xffff, 0xffffffff);

        private enum VaLoaderInterfaceStructs
        {
            VA_LOADER_INTERFACE_STRUCT_UNINITIALIZED = 0,
            VA_LOADER_INTERFACE_STRUCT_LOADER_INFO = 1,
            VA_LOADER_INTERFACE_STRUCT_RUNTIME_REQUEST = 3,
            VA_LOADER_INTERFACE_STRUCTS_MAX_ENUM = 0x7FFFFFFF,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VaNegotiateLoaderInfo
        {
            public VaLoaderInterfaceStructs structType; // VaLoaderInterfaceStructs
            public uint structVersion; // uint32_t
            public ulong structSize; // size_t
            public uint minInterfaceVersion; // uint32_t
            public uint maxInterfaceVersion; // uint32_t
            public VaVersion minApiVersion;
            public VaVersion maxApiVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VaNegotiateRuntimeRequest
        {
            public VaLoaderInterfaceStructs structType; // VaLoaderInterfaceStructs
            public uint structVersion; // uint32_t
            public ulong structSize; // size_t
            public uint runtimeInterfaceVersion; // uint32_t
            public VaVersion runtimeApiVersion; // VaVersion
            public IntPtr getFunctionPointer; // PFN_vaGetFunctionPointer
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate VaResult PFN_vaNegotiateLoaderRuntimeInterface(
            ref VaNegotiateLoaderInfo loaderInfo, // const VaNegotiateLoaderInfo *
            ref VaNegotiateRuntimeRequest runtimeRequest); // VaNegotiateRuntimeRequest *

        private struct RuntimePackageInfo
        {
            public string? PackageName;
            public string? RuntimeDllPath;
        }

        private static bool TryGetDefaultPackageInfo(ref RuntimePackageInfo info)
        {
            info.PackageName = @"Microsoft.MixedRealityVolumetric_8wekyb3d8bbwe";
            info.RuntimeDllPath = @"Platform\\VolumetricRuntime.dll";
            return true;
        }

        private static bool TryGetPackageInfo(ref RuntimePackageInfo info)
        {
            info.PackageName = RegistryHelper.GetStringValue(
                RegistryHelper.HKEY_CURRENT_USER,
                @"SOFTWARE\Microsoft\MixedReality\Volumetric",
                @"ActiveVolumetricPackage");
            if (string.IsNullOrEmpty(info.PackageName))
            {
                return false;   // Failed to get package name from regkey
            }

            info.RuntimeDllPath = RegistryHelper.GetStringValue(
                RegistryHelper.HKEY_CURRENT_USER,
                @"SOFTWARE\Microsoft\MixedReality\Volumetric",
                @"ActiveVolumetricRuntime");
            if (string.IsNullOrEmpty(info.RuntimeDllPath))
            {
                return false;   // Failed to get runtime DLL path from regkey
            }

            if (System.IO.Path.IsPathRooted(info.RuntimeDllPath))
            {
                return false;   // The dll path must be relative to the package
            }

            return true;
        }

        private static IntPtr LoadRuntimeDll()
        {
            IntPtr packageDependencyContext = IntPtr.Zero;
            try
            {
                RuntimePackageInfo packageInfo = new();
                if (!TryGetPackageInfo(ref packageInfo) && !TryGetDefaultPackageInfo(ref packageInfo))
                {
                    return IntPtr.Zero; // Failed to get package info
                }

                if (string.IsNullOrEmpty(packageInfo.PackageName) || string.IsNullOrEmpty(packageInfo.RuntimeDllPath))
                {
                    return IntPtr.Zero;   // Package name and runtime DLL path are required
                }

                bool result = PackageDependencyApi.CreateAndAddPackageDependency(packageInfo.PackageName!, out packageDependencyContext);
                if (!result)
                {
                    return IntPtr.Zero;  // Failed to create and add package dependency
                }

                return Win32Apis.LoadLibraryExW(packageInfo.RuntimeDllPath!, IntPtr.Zero, 0);
            }
            finally
            {
                if (packageDependencyContext != IntPtr.Zero)
                {
                    _ = PackageDependencyApi.RemovePackageDependency(packageDependencyContext);
                }
            }
        }

        private static VaResult vaNegotiateRuntime(out IntPtr vaGetFunctionPointer, VaVersion minApiVersion, VaVersion maxApiVersion)
        {
            vaGetFunctionPointer = IntPtr.Zero;
            IntPtr hModule = LoadRuntimeDll();
            if (hModule != IntPtr.Zero)
            {
                IntPtr pfn = Win32Apis.GetProcAddress(hModule, "vaNegotiateLoaderRuntimeInterface");
                if (pfn != IntPtr.Zero)
                {
                    PFN_vaNegotiateLoaderRuntimeInterface vaNegotiateLoaderRuntimeInterface = ToDelegate<PFN_vaNegotiateLoaderRuntimeInterface>(pfn);

                    var loaderInfo = new VaNegotiateLoaderInfo
                    {
                        structType = VaLoaderInterfaceStructs.VA_LOADER_INTERFACE_STRUCT_LOADER_INFO,
                        structVersion = VA_LOADER_INFO_STRUCT_VERSION,
                        structSize = (uint)Marshal.SizeOf<VaNegotiateLoaderInfo>(),
                        minInterfaceVersion = 1,
                        maxInterfaceVersion = VA_CURRENT_LOADER_RUNTIME_VERSION,
                        minApiVersion = minApiVersion,
                        maxApiVersion = maxApiVersion
                    };

                    var runtimeRequest = new VaNegotiateRuntimeRequest
                    {
                        structType = VaLoaderInterfaceStructs.VA_LOADER_INTERFACE_STRUCT_RUNTIME_REQUEST,
                        structVersion = VA_RUNTIME_INFO_STRUCT_VERSION,
                        structSize = (uint)Marshal.SizeOf<VaNegotiateRuntimeRequest>()
                    };

                    var vaResult = vaNegotiateLoaderRuntimeInterface(ref loaderInfo, ref runtimeRequest);
                    if (VaSucceeded(vaResult))
                    {
                        vaGetFunctionPointer = runtimeRequest.getFunctionPointer;
                        return VaResult.VA_SUCCESS;
                    }
                }
            }
            return VaResult.VA_ERROR_RUNTIME_UNAVAILABLE;
        }

        // To stay compatible with.netstandard 2.0 and above, use pinvoke instead of Microsoft.Win32.Registry.
        private static class RegistryHelper
        {
            // hKey handle
            internal static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(0x80000001);
            internal static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(0x80000002);

            internal static string? GetStringValue(IntPtr hKey, string subKey, string valueName, string defaultValue = "")
            {
                try
                {
                    IntPtr resultKey = IntPtr.Zero;
                    uint type = 0;
                    uint dataSize = 1024;
                    byte[] valueData = new byte[dataSize];

                    // Open the registry key
                    int result = RegistryHelper.RegOpenKeyExW(
                        hKey,
                        subKey,
                        0,
                        RegistryHelper.KEY_READ,
                        ref resultKey);

                    if (result != RegistryHelper.ERROR_SUCCESS)
                    {
                        return defaultValue;    // Failed to open the key
                    }

                    // Query the value from the opened key
                    result = RegistryHelper.RegQueryValueExW(
                        resultKey,
                        valueName,
                        IntPtr.Zero,
                        ref type,
                        valueData,
                        ref dataSize);

                    if (result == RegistryHelper.ERROR_SUCCESS)
                    {
                        // Convert byte[] to string (assuming the value is a string)
                        // Also trimming any null characters from the end
                        return Encoding.Unicode.GetString(valueData, 0, (int)dataSize).TrimEnd('\0');
                    }
                    else
                    {
                        return defaultValue;    // Failed to query the value
                    }
                }
                catch
                {
                    return defaultValue;
                }
            }

            // Constants for registry access
            private const int KEY_READ = 0x20019;
            private const int ERROR_SUCCESS = 0;

            // P/Invoke for RegOpenKeyExW
            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int RegOpenKeyExW(
                IntPtr hKey,
                string lpSubKey,
                uint ulOptions,
                uint samDesired,
                ref IntPtr phkResult);

            // P/Invoke for RegQueryValueExW
            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int RegQueryValueExW(
                IntPtr hKey,
                string lpValueName,
                IntPtr lpReserved,
                ref uint lpType,
                byte[] lpData,
                ref uint lpcbData);
        }

        private static class Win32Apis
        {
            private const string Kernel32 = "kernel32.dll";

            [DllImport(Kernel32, CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibraryExW(string filename, IntPtr hFile, int dwFlags);

            [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments"), PreserveSig]
            [DllImport(Kernel32, ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Ansi)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, [In, MarshalAs(UnmanagedType.LPStr)] string lpProcName);
        }

        private static class PackageDependencyApi
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct PackageVersion
            {
                public ulong Version;

                // Helper properties to access individual version components
                public ushort Revision => (ushort)(Version & 0xFFFF);
                public ushort Build => (ushort)((Version >> 16) & 0xFFFF);
                public ushort Minor => (ushort)((Version >> 32) & 0xFFFF);
                public ushort Major => (ushort)((Version >> 48) & 0xFFFF);

                // Constructor from individual components
                public PackageVersion(ushort major, ushort minor, ushort build, ushort revision)
                {
                    Version = ((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16) | revision;
                }

                public override string ToString()
                {
                    return $"{Major}.{Minor}.{Build}.{Revision}";
                }
            }

            public enum PackageDependencyLifetimeKind
            {
                Process = 0,
                FilePath = 1,
                RegistryKey = 2,
            }


            [Flags]
            public enum PackageDependencyProcessorArchitectures
            {
                None = 0,
                Neutral = 0x00000001,
                X86 = 0x00000002,
                X64 = 0x00000004,
                Arm = 0x00000008,
                Arm64 = 0x00000010,
                X86A64 = 0x00000020,
            }

            [Flags]
            public enum CreatePackageDependencyOptions
            {
                None = 0,
                DoNotVerifyDependencyResolution = 0x00000001,
                ScopeIsSystem = 0x00000002,
            }

            public enum AddPackageDependencyOptions
            {
                None = 0,
                PrependIfRankCollision = 1,
            }

            public enum PackagePathTypes
            {
                Install = 0,
                Mutable = 1,
                Effective = 2,
                MachineExternal = 3,
                UserExternal = 4,
                EffectiveExternal = 5,
            }


            internal static bool CreateAndAddPackageDependency(string package, out IntPtr packageDependencyContext)
            {
                packageDependencyContext = IntPtr.Zero;
                bool result = TryCreatePackageDependency(package, out var packageDependencyId);
                if (!result)
                {
                    return false;
                }

                string packageFullName = "";
                result = AddPackageDependency(packageDependencyId, out packageDependencyContext, out packageFullName);
                if (!result)
                {
                    return false;
                }

                return true;
            }
            internal static bool TryCreatePackageDependency(string package, out string packageDependencyId)
            {
                packageDependencyId = "";

                PackageDependencyProcessorArchitectures architecture = PackageDependencyProcessorArchitectures.None;
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    architecture = PackageDependencyProcessorArchitectures.X64;
                }
                else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    architecture = PackageDependencyProcessorArchitectures.Arm64;
                }
                else
                {
                    return false;
                }

                try
                {
                    int result = TryCreatePackageDependency(IntPtr.Zero,
                        package,
                        new PackageVersion(0, 0, 0, 0),
                        architecture,
                        PackageDependencyLifetimeKind.Process,
                        null!,
                        CreatePackageDependencyOptions.None,
                        out packageDependencyId);
                    return result == 0;
                }
                catch
                {
                    return false;
                }
            }

            internal static bool AddPackageDependency(string packageDependencyId, out IntPtr packageDependencyContext, out string packageFullName)
            {
                packageDependencyContext = IntPtr.Zero;
                int result = AddPackageDependency(packageDependencyId, 0, AddPackageDependencyOptions.None, out packageDependencyContext, out packageFullName);
                return result == 0;
            }


            [DllImport("kernelbase.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int TryCreatePackageDependency(IntPtr psid,
                [In, MarshalAs(UnmanagedType.LPWStr)] string packageFamilyName,
                PackageVersion minVersion,
                PackageDependencyProcessorArchitectures architecture,
                PackageDependencyLifetimeKind lifetimeKind,
                [In, MarshalAs(UnmanagedType.LPWStr)] string? lifetimeArtifact,
                CreatePackageDependencyOptions options,
                [Out, MarshalAs(UnmanagedType.LPWStr)] out string packageDependencyId);


            [DllImport("kernelbase.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int AddPackageDependency(
                [In, MarshalAs(UnmanagedType.LPWStr)] string packageDependencyId,
                int rank,
                AddPackageDependencyOptions options,
                out IntPtr packageDependencyContext,
                [Out, MarshalAs(UnmanagedType.LPWStr)] out string packageFullName);

            [DllImport("kernelbase.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern int RemovePackageDependency(IntPtr packageDependencyContext);

        }
    }
}
