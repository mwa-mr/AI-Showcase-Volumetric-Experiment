// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using System;
    using System.Runtime.InteropServices;
    using Api = Detail.Api;

    /// <summary>
    /// AdaptiveCard represents an interactive card that can be used to display information and receive user input.
    /// It can be customized with a template and data, and it can invoke actions when the user interacts with it.
    /// It follows the Adaptive Cards schema that can be referenced at https://adaptivecards.io/explorer.
    /// </summary>
    public class AdaptiveCard : Element
    {

        /// <summary>
        /// ActionEventArgs is used to pass the verb and data when an action is invoked on the AdaptiveCard.
        /// </summary>
        public class ActionEventArgs : EventArgs
        {
            /// <summary>
            /// Gets the verb associated with the action invoked.
            /// </summary>
            public string Verb { get; private set; }

            /// <summary>
            /// Gets the data associated with the action invoked.
            /// </summary>
            public string Data { get; private set; }

            /// <summary>
            /// Creates a new instance of ActionEventArgs with the specified verb and data.
            /// </summary>
            public ActionEventArgs(string verb, string data)
            {
                Verb = verb;
                Data = data;
            }
        }

        /// <summary>
        /// Event that is raised when an action is invoked on the AdaptiveCard.
        /// </summary>
        public event EventHandler<ActionEventArgs>? ActionInvoked;

        /// <summary>
        /// Creates a new AdaptiveCard in the volume with the specified template, and data.
        /// The template and data are optional and can be set later using SetTemplate and SetData methods.
        /// </summary>
        public AdaptiveCard(Volume volume, string? template = default, string? data = default)
            : base(VaElementType.AdaptiveCardExt, volume, CreateElement)
        {
            SetTemplate(template);
            SetData(data);
        }

        /// <summary>
        /// Sets the template for the AdaptiveCard.
        /// The template should be a valid Adaptive Card JSON string. If null is passed, it will set an empty template.
        /// </summary>
        public void SetTemplate(string? template)
        {
            SetPropertyString(VaElementProperty.AdaptiveCardTemplateExt, template ?? string.Empty);
        }

        /// <summary>
        /// Sets the data for the AdaptiveCard.
        /// The data should be a valid JSON string that conforms to the Adaptive Card schema. If null is passed, it will set an empty data.
        /// </summary>
        public void SetData(string? data)
        {
            SetPropertyString(VaElementProperty.AdaptiveCardDataExt, data ?? string.Empty);
        }

        internal void PollAdaptiveCardActionInvokedData()
        {
            var actionData = new Api.VaAdaptiveCardActionInvokedDataExt
            {
                type = Api.VaStructureType.VA_TYPE_ADAPTIVE_CARD_ACTION_INVOKED_DATA_EXT
            };

            // Two call idiom. This will tell us how much data we need to allocate for the second call.
            Api.CheckResult(Api.vaGetNextAdaptiveCardActionInvokedDataExt(Handle, out actionData));

            while (actionData.verbCountOutput > 0 || actionData.dataCountOutput > 0)
            {
                IntPtr verbBuffer = IntPtr.Zero;
                IntPtr dataBuffer = IntPtr.Zero;

                try
                {
                    // Allocate enough memory to hold the two output strings. This must include data for the null terminator. *CountOutput accounts for it.
                    verbBuffer = Marshal.AllocHGlobal((int)actionData.verbCountOutput);
                    actionData.verb = verbBuffer;
                    actionData.verbCapacityInput = actionData.verbCountOutput;

                    dataBuffer = Marshal.AllocHGlobal((int)actionData.dataCountOutput);
                    actionData.data = dataBuffer;
                    actionData.dataCapacityInput = actionData.dataCountOutput;

                    Api.CheckResult(Api.vaGetNextAdaptiveCardActionInvokedDataExt(Handle, out actionData));

                    if (actionData.hasData == (VaBool32)1)
                    {
                        ActionInvoked?.Invoke(this, new ActionEventArgs(Marshal.PtrToStringAnsi(verbBuffer), Marshal.PtrToStringAnsi(dataBuffer)));
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(verbBuffer);
                    Marshal.FreeHGlobal(dataBuffer);
                }

                // Now see if there are any more actions. Exit loop if output is empty.
                actionData = new Api.VaAdaptiveCardActionInvokedDataExt
                {
                    type = Api.VaStructureType.VA_TYPE_ADAPTIVE_CARD_ACTION_INVOKED_DATA_EXT
                };
                Api.CheckResult(Api.vaGetNextAdaptiveCardActionInvokedDataExt(Handle, out actionData));
            }
        }
    }
}
