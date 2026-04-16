# Using wprp files to monitor API errors

The Volumetric SDK includes `wprp` files to monitor API errors in real-time. These files also provide a large number of details to assist you with debugging. You can use them to track errors with the Instant Trace Viewer (ITV). The ITV tool is an open-source app for monitoring Event Tracing for Windows (ETW) traces in real-time. It's available from the [Microsoft Store](https://apps.microsoft.com/detail/9NWPWZGRVL2C) and instructions on how to use it are on [GitHub](https://github.com/brycehutchings/InstantTraceViewer?tab=readme-ov-file#instant-trace-viewer). There are other trace application options that you may be able to use, but we recommend ITV.

These are the `VaSDK` files you can use with a trace viewer application. They each have different features:

 - The `VaSDK.wprp` file starts when you press F5 to run your Volumetric app. API runtime traces will immediately display debugging information. This file is useful for standard tracing procedures.

- The `VaSDK.Continuous.wprp` file provides more verbose output and additional traces that are useful for deeper debugging scenarios. This file is best used when you need more extensive tracing.

- The `VaSDK.Perf.wprp` file captures only the `Microsoft.MixedReality.Volumetric.Platform.Perf` provider at Verbose level with the PERF keyword. Use this file for lightweight performance tracing without the noise of other providers.

All `wprp` and trace applications will display error information if an API returns an error code. When you inspect these detailed error messages and related traces, you can diagnose what went wrong and adjust your application code accordingly. 

**NOTE:** To report a platform or product usage bug, please use the [Feedback Hub](https://aka.ms/WIPFeedbackHub).