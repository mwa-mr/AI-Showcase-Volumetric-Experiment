// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
// This file is generated from spec.xml

namespace Microsoft.MixedReality.Volumetric.Detail
{
    using System;
    using System.Runtime.InteropServices;
    internal partial class Api
    {
#nullable disable
        public static PFN_vaGetFunctionPointer vaGetFunctionPointer;
        public static PFN_vaEnumerateExtensions vaEnumerateExtensions;
        public static PFN_vaCreateSession vaCreateSession;
        public static PFN_vaDestroySession vaDestroySession;
        public static PFN_vaRequestStopSession vaRequestStopSession;
        public static PFN_vaPollEvent vaPollEvent;
        public static PFN_vaWaitForNextEvent vaWaitForNextEvent;
        public static PFN_vaWaitEvent vaWaitEvent;
        public static PFN_vaCreateVolume vaCreateVolume;
        public static PFN_vaDestroyVolume vaDestroyVolume;
        public static PFN_vaRequestCloseVolume vaRequestCloseVolume;
        public static PFN_vaRequestUpdateVolume vaRequestUpdateVolume;
        public static PFN_vaBeginUpdateVolume vaBeginUpdateVolume;
        public static PFN_vaEndUpdateVolume vaEndUpdateVolume;
        public static PFN_vaCreateElement vaCreateElement;
        public static PFN_vaDestroyElement vaDestroyElement;
        public static PFN_vaGetElementPropertyBool vaGetElementPropertyBool;
        public static PFN_vaGetElementPropertyEnum vaGetElementPropertyEnum;
        public static PFN_vaGetElementPropertyFloat vaGetElementPropertyFloat;
        public static PFN_vaGetElementPropertyVector3f vaGetElementPropertyVector3f;
        public static PFN_vaGetElementPropertyQuaternionf vaGetElementPropertyQuaternionf;
        public static PFN_vaGetElementPropertyExtent3Df vaGetElementPropertyExtent3Df;
        public static PFN_vaSetElementPropertyBool vaSetElementPropertyBool;
        public static PFN_vaSetElementPropertyEnum vaSetElementPropertyEnum;
        public static PFN_vaSetElementPropertyFlags vaSetElementPropertyFlags;
        public static PFN_vaSetElementPropertyString vaSetElementPropertyString;
        public static PFN_vaSetElementPropertyUInt32 vaSetElementPropertyUInt32;
        public static PFN_vaSetElementPropertyInt32 vaSetElementPropertyInt32;
        public static PFN_vaSetElementPropertyHandle vaSetElementPropertyHandle;
        public static PFN_vaSetElementPropertyFloat vaSetElementPropertyFloat;
        public static PFN_vaSetElementPropertyVector3f vaSetElementPropertyVector3f;
        public static PFN_vaSetElementPropertyColor3f vaSetElementPropertyColor3f;
        public static PFN_vaSetElementPropertyColor4f vaSetElementPropertyColor4f;
        public static PFN_vaSetElementPropertyQuaternionf vaSetElementPropertyQuaternionf;
        public static PFN_vaSetElementPropertyExtent3Df vaSetElementPropertyExtent3Df;
        public static PFN_vaGetNextElementAsyncError vaGetNextElementAsyncError;
        public static PFN_vaGetChangedElements vaGetChangedElements;
        public static PFN_vaLocateSpacesExt vaLocateSpacesExt;
        public static PFN_vaLocateJointsExt vaLocateJointsExt;
        public static PFN_vaGetVolumeRestoreIdExt vaGetVolumeRestoreIdExt;
        public static PFN_vaRemoveRestorableVolumeExt vaRemoveRestorableVolumeExt;
        public static PFN_vaGetNextAdaptiveCardActionInvokedDataExt vaGetNextAdaptiveCardActionInvokedDataExt;
        public static PFN_vaAcquireMeshBufferExt vaAcquireMeshBufferExt;
        public static PFN_vaReleaseMeshBufferExt vaReleaseMeshBufferExt;
#nullable restore

        internal static void InitializeFunctionPointers(IntPtr session, PFN_vaGetFunctionPointer getFunctionPointer, SessionExtensions extensions)
        {
            vaGetFunctionPointer = getFunctionPointer;
            vaGetFunctionPointer = Api.GetFunctionPointer<PFN_vaGetFunctionPointer>(session, "vaGetFunctionPointer");
            vaEnumerateExtensions = Api.GetFunctionPointer<PFN_vaEnumerateExtensions>(session, "vaEnumerateExtensions");
            vaCreateSession = Api.GetFunctionPointer<PFN_vaCreateSession>(session, "vaCreateSession");
            vaDestroySession = Api.GetFunctionPointer<PFN_vaDestroySession>(session, "vaDestroySession");
            vaRequestStopSession = Api.GetFunctionPointer<PFN_vaRequestStopSession>(session, "vaRequestStopSession");
            vaPollEvent = Api.GetFunctionPointer<PFN_vaPollEvent>(session, "vaPollEvent");
            vaWaitForNextEvent = Api.GetFunctionPointer<PFN_vaWaitForNextEvent>(session, "vaWaitForNextEvent");
            vaWaitEvent = Api.GetFunctionPointer<PFN_vaWaitEvent>(session, "vaWaitEvent");
            vaCreateVolume = Api.GetFunctionPointer<PFN_vaCreateVolume>(session, "vaCreateVolume");
            vaDestroyVolume = Api.GetFunctionPointer<PFN_vaDestroyVolume>(session, "vaDestroyVolume");
            vaRequestCloseVolume = Api.GetFunctionPointer<PFN_vaRequestCloseVolume>(session, "vaRequestCloseVolume");
            vaRequestUpdateVolume = Api.GetFunctionPointer<PFN_vaRequestUpdateVolume>(session, "vaRequestUpdateVolume");
            vaBeginUpdateVolume = Api.GetFunctionPointer<PFN_vaBeginUpdateVolume>(session, "vaBeginUpdateVolume");
            vaEndUpdateVolume = Api.GetFunctionPointer<PFN_vaEndUpdateVolume>(session, "vaEndUpdateVolume");
            vaCreateElement = Api.GetFunctionPointer<PFN_vaCreateElement>(session, "vaCreateElement");
            vaDestroyElement = Api.GetFunctionPointer<PFN_vaDestroyElement>(session, "vaDestroyElement");
            vaGetElementPropertyBool = Api.GetFunctionPointer<PFN_vaGetElementPropertyBool>(session, "vaGetElementPropertyBool");
            vaGetElementPropertyEnum = Api.GetFunctionPointer<PFN_vaGetElementPropertyEnum>(session, "vaGetElementPropertyEnum");
            vaGetElementPropertyFloat = Api.GetFunctionPointer<PFN_vaGetElementPropertyFloat>(session, "vaGetElementPropertyFloat");
            vaGetElementPropertyVector3f = Api.GetFunctionPointer<PFN_vaGetElementPropertyVector3f>(session, "vaGetElementPropertyVector3f");
            vaGetElementPropertyQuaternionf = Api.GetFunctionPointer<PFN_vaGetElementPropertyQuaternionf>(session, "vaGetElementPropertyQuaternionf");
            vaGetElementPropertyExtent3Df = Api.GetFunctionPointer<PFN_vaGetElementPropertyExtent3Df>(session, "vaGetElementPropertyExtent3Df");
            vaSetElementPropertyBool = Api.GetFunctionPointer<PFN_vaSetElementPropertyBool>(session, "vaSetElementPropertyBool");
            vaSetElementPropertyEnum = Api.GetFunctionPointer<PFN_vaSetElementPropertyEnum>(session, "vaSetElementPropertyEnum");
            vaSetElementPropertyFlags = Api.GetFunctionPointer<PFN_vaSetElementPropertyFlags>(session, "vaSetElementPropertyFlags");
            vaSetElementPropertyString = Api.GetFunctionPointer<PFN_vaSetElementPropertyString>(session, "vaSetElementPropertyString");
            vaSetElementPropertyUInt32 = Api.GetFunctionPointer<PFN_vaSetElementPropertyUInt32>(session, "vaSetElementPropertyUInt32");
            vaSetElementPropertyInt32 = Api.GetFunctionPointer<PFN_vaSetElementPropertyInt32>(session, "vaSetElementPropertyInt32");
            vaSetElementPropertyHandle = Api.GetFunctionPointer<PFN_vaSetElementPropertyHandle>(session, "vaSetElementPropertyHandle");
            vaSetElementPropertyFloat = Api.GetFunctionPointer<PFN_vaSetElementPropertyFloat>(session, "vaSetElementPropertyFloat");
            vaSetElementPropertyVector3f = Api.GetFunctionPointer<PFN_vaSetElementPropertyVector3f>(session, "vaSetElementPropertyVector3f");
            vaSetElementPropertyColor3f = Api.GetFunctionPointer<PFN_vaSetElementPropertyColor3f>(session, "vaSetElementPropertyColor3f");
            vaSetElementPropertyColor4f = Api.GetFunctionPointer<PFN_vaSetElementPropertyColor4f>(session, "vaSetElementPropertyColor4f");
            vaSetElementPropertyQuaternionf = Api.GetFunctionPointer<PFN_vaSetElementPropertyQuaternionf>(session, "vaSetElementPropertyQuaternionf");
            vaSetElementPropertyExtent3Df = Api.GetFunctionPointer<PFN_vaSetElementPropertyExtent3Df>(session, "vaSetElementPropertyExtent3Df");
            vaGetNextElementAsyncError = Api.GetFunctionPointer<PFN_vaGetNextElementAsyncError>(session, "vaGetNextElementAsyncError");
            vaGetChangedElements = Api.GetFunctionPointer<PFN_vaGetChangedElements>(session, "vaGetChangedElements");

            if (extensions.IsEnabled(Extensions.VA_EXT_locate_spaces))
            {
                vaLocateSpacesExt = Api.GetFunctionPointer<PFN_vaLocateSpacesExt>(session, "vaLocateSpacesExt");
            }

            if (extensions.IsEnabled(Extensions.VA_EXT_locate_joints))
            {
                vaLocateJointsExt = Api.GetFunctionPointer<PFN_vaLocateJointsExt>(session, "vaLocateJointsExt");
            }

            if (extensions.IsEnabled(Extensions.VA_EXT_volume_restore))
            {
                vaGetVolumeRestoreIdExt = Api.GetFunctionPointer<PFN_vaGetVolumeRestoreIdExt>(session, "vaGetVolumeRestoreIdExt");
                vaRemoveRestorableVolumeExt = Api.GetFunctionPointer<PFN_vaRemoveRestorableVolumeExt>(session, "vaRemoveRestorableVolumeExt");
            }

            if (extensions.IsEnabled(Extensions.VA_EXT_adaptive_card_element))
            {
                vaGetNextAdaptiveCardActionInvokedDataExt = Api.GetFunctionPointer<PFN_vaGetNextAdaptiveCardActionInvokedDataExt>(session, "vaGetNextAdaptiveCardActionInvokedDataExt");
            }

            if (extensions.IsEnabled(Extensions.VA_EXT_mesh_edit))
            {
                vaAcquireMeshBufferExt = Api.GetFunctionPointer<PFN_vaAcquireMeshBufferExt>(session, "vaAcquireMeshBufferExt");
                vaReleaseMeshBufferExt = Api.GetFunctionPointer<PFN_vaReleaseMeshBufferExt>(session, "vaReleaseMeshBufferExt");
            }

        }

    }
}
