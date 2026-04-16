// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

using System;

namespace Microsoft.MixedReality.Volumetric
{
    public static class VaMath
    {
        private static readonly VaVector3f _zero = new VaVector3f() { x = 0, y = 0, z = 0 };
        private static readonly VaVector3f _one = new VaVector3f() { x = 1, y = 1, z = 1 };

        private static readonly VaVector3f _right = new VaVector3f() { x = 1, y = 0, z = 0 };
        private static readonly VaVector3f _up = new VaVector3f() { x = 0, y = 1, z = 0 };
        private static readonly VaVector3f _back = new VaVector3f() { x = 0, y = 0, z = 1 };
        private static readonly VaVector3f _forward = new VaVector3f() { x = 0, y = 0, z = -1 };
        private static readonly VaVector3f _down = new VaVector3f() { x = 0, y = -1, z = 0 };
        private static readonly VaVector3f _left = new VaVector3f() { x = -1, y = 0, z = 0 };

        private static readonly VaQuaternionf _identity = new VaQuaternionf() { x = 0, y = 0, z = 0, w = 1 };

        private static readonly VaExtent3Df _zeroSize = new VaExtent3Df() { width = 0, height = 0, depth = 0 };
        private static readonly VaExtent3Df _oneSize = new VaExtent3Df() { width = 1, height = 1, depth = 1 };

        /// <summary>
        /// Gets the zero vector (0, 0, 0).
        /// </summary>
        public static VaVector3f Zero => _zero;
        /// <summary>
        /// Gets the one vector (1, 1, 1).
        /// </summary>
        public static VaVector3f One => _one;

        /// <summary>
        /// Gets a direction vector pointing to the user's right (1, 0, 0).
        /// </summary>
        public static VaVector3f Right => _right;
        /// <summary>
        /// Gets a direction vector pointing to the user's up (0, 1, 0).
        /// </summary>
        public static VaVector3f Up => _up;
        /// <summary>
        /// Gets a direction vector pointing to the user's back (0, 0, 1).
        /// </summary>
        public static VaVector3f Back => _back;
        /// <summary>
        /// Gets a direction vector pointing to the user's forward (0, 0, -1).
        /// </summary>
        public static VaVector3f Forward => _forward;
        /// <summary>
        /// Gets a direction vector pointing to the user's down (0, -1, 0).
        /// </summary>
        public static VaVector3f Down => _down;
        /// <summary>
        /// Gets a direction vector pointing to the user's left (-1, 0, 0).
        /// </summary>
        public static VaVector3f Left => _left;

        /// <summary>
        /// Gets the identity quaternion (0, 0, 0, 1).
        /// </summary>
        public static VaQuaternionf Identity => _identity;

        /// <summary>
        /// Gets a zero size extent (0, 0, 0).
        /// </summary>
        public static VaExtent3Df ZeroSize => _zeroSize;
        /// <summary>
        /// Gets a one size extent (1, 1, 1).
        /// </summary>
        public static VaExtent3Df OneSize => _oneSize;

        /// <summary>
        /// Returns the linear interpolation of two vectors.
        /// When t = 0, the result is a. When t = 1, the result is b.
        /// </summary>
        public static VaVector3f Lerp(VaVector3f a, VaVector3f b, float t)
        {
            return new VaVector3f()
            {
                x = a.x + (b.x - a.x) * t,
                y = a.y + (b.y - a.y) * t,
                z = a.z + (b.z - a.z) * t
            };
        }

        /// <summary>
        /// Returns the quaternions represents the combined rotation of p and then q.
        /// </summary>
        public static VaQuaternionf Multiply(VaQuaternionf p, VaQuaternionf q)
        {
            return new VaQuaternionf()
            {
                x = p.w * q.x + p.x * q.w + p.y * q.z - p.z * q.y,
                y = p.w * q.y - p.x * q.z + p.y * q.w + p.z * q.x,
                z = p.w * q.z + p.x * q.y - p.y * q.x + p.z * q.w,
                w = p.w * q.w - p.x * q.x - p.y * q.y - p.z * q.z
            };
        }

        /// <summary>
        /// Returns the quaternion represents the rotation of Eular angles x, then y, then z.
        /// The angles are in radians.
        /// </summary>
        /// <param name="x">Rotation around the x-axis (pitch)</param>
        /// <param name="y">Rotation around the y-axis (yaw)</param>
        /// <param name="z">Rotation around the z-axis (roll)</param>
        public static VaQuaternionf EulerToQuaternion(float x, float y, float z)
        {
            // Convert half angles
            float cx = (float)Math.Cos(x * 0.5f);
            float sx = (float)Math.Sin(x * 0.5f);
            float cy = (float)Math.Cos(y * 0.5f);
            float sy = (float)Math.Sin(y * 0.5f);
            float cz = (float)Math.Cos(z * 0.5f);
            float sz = (float)Math.Sin(z * 0.5f);

            // Quaternion calculation using intrinsic Tait-Bryan angles (XYZ order)
            return new VaQuaternionf()
            {
                x = sx * cy * cz - cx * sy * sz,
                y = cx * sy * cz + sx * cy * sz,
                z = cx * cy * sz - sx * sy * cz,
                w = cx * cy * cz + sx * sy * sz
            };
        }
    }
}
