// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using Detail;
    using System;
    using System.Threading;
    using Api = Detail.Api;

    public abstract class Element
    {
        readonly private IntPtr _handle;
        readonly private VaElementType _type;
        readonly private Volume _volume;
        internal VaElementAsyncState _asyncState = VaElementAsyncState.Ready;
        private int _isDestroyed;    // Atomic flag that will only be set 1 once in Destroy() and otherwise will remain 0.


        internal IntPtr Handle => _handle;

        /// <summary>
        /// Gets the type of the element.  This won't change after the element is created.
        /// </summary>
        public VaElementType ElementType => _type;

        /// <summary>
        /// Gets the volume that this element belongs to.  This won't change after the element is created.
        /// </summary>
        public Volume Volume => _volume;

        /// <summary>
        /// Creates a new element of the specified type in the specified volume.
        /// This constructor is protected and should only be called by derived classes.
        /// </summary>
        protected Element(VaElementType type, Volume volume, Func<VaElementType, Volume, IntPtr> createElement, VaElementAsyncState defaultAsyncState = VaElementAsyncState.Ready)
        {
            _type = type;
            _volume = volume;
            _handle = createElement(type, volume);
            _asyncState = defaultAsyncState;

            volume.AddElement(this);
        }

        /// <summary>
        /// Creates a new element raw handle of the specified type in the specified volume.
        /// This method is protected and should only be called by derived classes.
        /// </summary>
        static protected IntPtr CreateElement(VaElementType type, Volume volume)
        {
            var createInfo = new Api.VaElementCreateInfo
            {
                type = Api.VaStructureType.VA_TYPE_ELEMENT_CREATE_INFO,
                elementType = (Api.VaElementType)type,
            };

            IntPtr handle;
            Api.CheckResult(Api.vaCreateElement(volume.Handle, createInfo, out handle));
            Trace.LogInfo(() => $"create_element: element = {handle}");
            return handle;
        }

        /// <summary>
        /// Gets whether the element is ready to be used.
        /// When false, the element is not functioning and app should avoid setting properties or calling methods on it.
        /// </summary>
        /// <remarks> When IsReady is false, the element may be in a pending state or an error state. </remarks>
        public bool IsReady => _asyncState == VaElementAsyncState.Ready;

        /// <summary>
        /// Gets whether the element is in a pending state.
        /// When true, there is a pending asynchronous operation on the element.
        /// Any new operations on the elment will be queued until the pending operation is complete.
        /// </summary>
        public bool IsPending => _asyncState == VaElementAsyncState.Pending;

        /// <summary>
        /// Gets whether the element is in an error state.
        /// When true, the element is not functioning and app should avoid setting properties or calling methods on it.
        /// The app can use the GetNextError() method to get all errors occurred on the element in current frame.
        /// </summary>
        public bool HasError => _asyncState == VaElementAsyncState.Error;

        /// <summary>
        /// The event that is called when the async state of the element changes.
        /// The event is called on the event loop thread and always before onUpdate() is called.
        /// </summary>
        public Action<VaElementAsyncState>? OnAsyncStateChanged { get; set; }

        internal void UpdateAsyncState()
        {
            int value;
            Api.CheckResult(Api.vaGetElementPropertyEnum(Handle, Api.VaElementProperty.VA_ELEMENT_PROPERTY_ASYNC_STATE, out value));
            VaElementAsyncState newValue = (VaElementAsyncState)value;

            if (_asyncState != newValue)
            {
                _asyncState = newValue;
                OnAsyncStateChanged?.Invoke(_asyncState);
            }
        }

        /// <summary>
        /// Gets all asynchronous error that occurred on the element in current frame.
        /// the input onError callback will be called with each error once and only once.
        /// The queue of errors is cleared after this method is called.
        /// If the app didn't call this method, the errors will be cleared on the next frame.
        /// </summary>
        public void GetAsyncErrors(Action<VaElementAsyncError, string> onError)
        {
            if (onError == null)
            {
                return;
            }

            var errorData = new Api.VaElementAsyncErrorData
            {
                type = Api.VaStructureType.VA_TYPE_ELEMENT_ASYNC_ERROR_DATA,
            };

            while (true)
            {
                Api.CheckResult(Api.vaGetNextElementAsyncError(Handle, ref errorData));
                if (errorData.error == Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_NO_MORE)
                {
                    break;
                }

                try
                {
                    onError((VaElementAsyncError)errorData.error, errorData.errorMessage);
                }
                catch (Exception e)
                {
                    Trace.LogWarning(() => $"Exception caught in GetAsyncErrors callback: {e.Message}");
                }
            }
        }

        protected internal void SetPropertyBool(VaElementProperty property, bool value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyBool(Handle, p, value ? (VaBool32)1 : (VaBool32)0));
        }

        protected internal void SetPropertyElement(VaElementProperty property, Element value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyHandle(Handle, p, value.Handle));
        }

        protected internal void SetPropertyFloat(VaElementProperty property, float value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyFloat(Handle, p, value));
        }

        protected internal void SetPropertyVector3f(VaElementProperty property, in VaVector3f value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyVector3f(Handle, p, value));
        }

        protected internal void SetPropertyQuaternionf(VaElementProperty property, in VaQuaternionf value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyQuaternionf(Handle, p, value));
        }

        protected internal void SetPropertyColor4f(VaElementProperty property, in VaColor4f value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyColor4f(Handle, p, value));
        }

        protected internal void SetPropertyString(VaElementProperty property, in string value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyString(Handle, p, value));
        }

        protected internal void SetPropertyExtent3Df(VaElementProperty property, in VaExtent3Df value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyExtent3Df(Handle, p, value));
        }

        protected internal void SetPropertyEnum(VaElementProperty property, in Int32 value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyEnum(Handle, p, value));
        }

        protected internal void SetPropertyFlags(VaElementProperty property, in UInt32 value)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaSetElementPropertyFlags(Handle, p, value));
        }

        protected internal bool GetPropertyBool(VaElementProperty property)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaGetElementPropertyBool(Handle, p, out VaBool32 value));
            return value != 0;
        }

        protected internal float GetPropertyFloat(VaElementProperty property)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaGetElementPropertyFloat(Handle, p, out float value));
            return value;
        }

        protected internal VaVector3f GetPropertyVector3f(VaElementProperty property)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaGetElementPropertyVector3f(Handle, p, out VaVector3f value));
            return value;
        }

        protected internal VaQuaternionf GetPropertyQuaternionf(VaElementProperty property)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaGetElementPropertyQuaternionf(Handle, p, out VaQuaternionf value));
            return value;
        }

        protected internal VaExtent3Df GetPropertyExtent3Df(VaElementProperty property)
        {
            Api.VaElementProperty p = (Api.VaElementProperty)property;
            Api.CheckResult(Api.vaGetElementPropertyExtent3Df(Handle, p, out VaExtent3Df value));
            return value;
        }

        /// <summary>
        /// Destroys the element handle and related resources.
        /// This method is called automatically when the volume is closed.
        /// If the app needs to destroy the element before closing the volume, it should call this method.
        /// </summary>
        public void Destroy()
        {
            if (Interlocked.CompareExchange(ref _isDestroyed, 1, 0) == 0)
            {
                if (Volume != null)
                {
                    //Disconnect from parent volume.
                    Volume.RemoveElement(this);
                }

                if (_handle != IntPtr.Zero)
                {
                    Api.vaDestroyElement(_handle);
                }
            }
        }

        // When the parent volume already removed this element, just destroy the handle is enough.
        internal void DestroyHandle()
        {
            if (Interlocked.CompareExchange(ref _isDestroyed, 1, 0) == 0)
            {
                if (_handle != IntPtr.Zero)
                {
                    Api.vaDestroyElement(_handle);
                }
            }
        }
    }
}
