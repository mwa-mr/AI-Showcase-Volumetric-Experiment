// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

using System;
using System.Runtime.InteropServices;

namespace Microsoft.MixedReality.Volumetric
{
    /// <summary>
    /// Represents a 128-bit universally unique identifier (UUID).
    /// Unlike <see cref="System.Guid"/>, VaUuid uses RFC 4122 byte ordering 
    /// (big-endian/network byte order for the first three fields).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VaUuid : IEquatable<VaUuid>, IComparable<VaUuid>, IFormattable
    {
        private UuidData m_data;

        /// <summary>
        /// A read-only instance of <see cref="VaUuid"/> whose value is all zeros.
        /// This is typically used to represent an uninitialized or invalid UUID.
        /// </summary>
        public static readonly VaUuid Empty;

        /// <summary>
        /// Create a new <see cref="VaUuid"/> by parsing the provided string representation of a UUID.
        /// The string format should match that of a standard GUID, e.g. "123e4567-e89b-12d3-a456-426614174000"
        /// or "{123e4567-e89b-12d3-a456-426614174000}" (with curly braces).
        /// </summary>
        /// <param name="str">A string containing a UUID to convert.</param>
        /// <returns>A <see cref="VaUuid"/> equivalent to the UUID contained in <paramref name="str"/>, or <see cref="Empty"/> if parsing fails.</returns>
        public static VaUuid FromString(string str)
        {
            _ = TryParse(str, out VaUuid result);
            return result;
        }

        /// <summary>
        /// Attempts to create a new <see cref="VaUuid"/> by parsing the provided string representation of a UUID.
        /// The string format should match that of a standard GUID, e.g. "123e4567-e89b-12d3-a456-426614174000"
        /// or "{123e4567-e89b-12d3-a456-426614174000}" (with curly braces).
        /// </summary>
        /// <param name="str">A string containing the UUID to convert.</param>
        /// <param name="result">When this method returns, contains the parsed value on success, or <see cref="Empty"/> on failure.</param>
        /// <returns><see langword="true"/> if the parse operation was successful; otherwise, <see langword="false"/>.</returns>
        public static bool TryParse(string str, out VaUuid result)
        {
            if (Guid.TryParse(str, out Guid g))
            {
                result = default;
                result.m_data = UuidData.FromGuid(g);
                return true;
            }
            result = Empty;
            return false;
        }

        /// <summary>
        /// Returns the string representation of this UUID.
        /// </summary>
        /// <remarks>
        /// The format is "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" (32 hexadecimal digits with hyphens, no curly braces).
        /// This is equivalent to calling <see cref="ToString(string)"/> with "D" or <see langword="null"/>.
        /// </remarks>
        /// <returns>A string representation of this UUID, e.g. "123e4567-e89b-12d3-a456-426614174000".</returns>
        public override string ToString() => m_data.FormatString(null, null);

        /// <summary>
        /// Returns the string representation of this UUID using the specified format.
        /// </summary>
        /// <param name="format">A format specifier indicating how to format the UUID.
        /// If <see langword="null"/> or empty, the default "D" format is used.</param>
        /// <returns>The string representation of this UUID in the specified format.</returns>
        /// <exception cref="FormatException">Thrown when <paramref name="format"/> is not a recognized format specifier.</exception>
        /// <remarks>
        /// The supported format specifiers match <see cref="Guid.ToString(string)"/>:
        /// <list type="bullet">
        /// <item><description>"N": 32 hexadecimal digits without hyphens: "00000000000000000000000000000000"</description></item>
        /// <item><description>"D" or <see langword="null"/>: 32 hexadecimal digits with hyphens (default): "00000000-0000-0000-0000-000000000000"</description></item>
        /// <item><description>"B": 32 hexadecimal digits with hyphens, enclosed in braces: "{00000000-0000-0000-0000-000000000000}"</description></item>
        /// <item><description>"P": 32 hexadecimal digits with hyphens, enclosed in parentheses: "(00000000-0000-0000-0000-000000000000)"</description></item>
        /// <item><description>"X": Four hexadecimal values enclosed in braces: "{0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}"</description></item>
        /// </list>
        /// </remarks>
        public string ToString(string? format)
        {
            return m_data.FormatString(format, null);
        }

        /// <inheritdoc/>
        public string ToString(string? format, IFormatProvider? formatProvider) => m_data.FormatString(format, formatProvider);

        /// <summary>
        /// Determines whether the specified object is equal to this UUID.
        /// </summary>
        /// <param name="obj">The object to compare with this UUID.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="VaUuid"/> and has the same byte-wise value; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is VaUuid other && Equals(other);

        /// <summary>
        /// Determines whether the specified UUID is equal to this UUID by performing a byte-wise comparison.
        /// </summary>
        /// <param name="other">The UUID to compare with this UUID.</param>
        /// <returns><see langword="true"/> if both UUIDs have identical byte sequences; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VaUuid other) => m_data.AsSpan().SequenceEqual(other.m_data.AsSpan());

        /// <summary>
        /// Compares this UUID with another UUID using byte-wise lexicographic ordering.
        /// </summary>
        /// <param name="other">The UUID to compare with this UUID.</param>
        /// <returns>A value less than zero if this UUID precedes <paramref name="other"/>, zero if they are equal, 
        /// or a value greater than zero if this UUID follows <paramref name="other"/>.</returns>
        public int CompareTo(VaUuid other) => m_data.AsSpan().SequenceCompareTo(other.m_data.AsSpan());

        /// <inheritdoc/>
        public override int GetHashCode() => m_data.ComputeHashCode();

        /// <summary>Determines whether two specified <see cref="VaUuid"/> values are equal.</summary>
        public static bool operator ==(VaUuid left, VaUuid right) => left.Equals(right);

        /// <summary>Determines whether two specified <see cref="VaUuid"/> values are not equal.</summary>
        public static bool operator !=(VaUuid left, VaUuid right) => !left.Equals(right);

        /// <summary>Determines whether one specified <see cref="VaUuid"/> is less than another.</summary>
        public static bool operator <(VaUuid left, VaUuid right) => left.CompareTo(right) < 0;

        /// <summary>Determines whether one specified <see cref="VaUuid"/> is less than or equal to another.</summary>
        public static bool operator <=(VaUuid left, VaUuid right) => left.CompareTo(right) <= 0;

        /// <summary>Determines whether one specified <see cref="VaUuid"/> is greater than another.</summary>
        public static bool operator >(VaUuid left, VaUuid right) => left.CompareTo(right) > 0;

        /// <summary>Determines whether one specified <see cref="VaUuid"/> is greater than or equal to another.</summary>
        public static bool operator >=(VaUuid left, VaUuid right) => left.CompareTo(right) >= 0;

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private unsafe struct UuidData
        {
            private fixed byte data[16];

            public ReadOnlySpan<byte> AsSpan()
            {
                fixed (byte* ptr = data)
                {
                    return new ReadOnlySpan<byte>(ptr, 16);
                }
            }

            public string FormatString(string? format, IFormatProvider? formatProvider)
            {
                return ToGuid().ToString(format, formatProvider);
            }

            public int ComputeHashCode()
            {
                fixed (byte* ptr = data)
                {
                    // Treat as two 64-bit integers for efficient hashing
                    long* longPtr = (long*)ptr;
                    return HashCode.Combine(longPtr[0], longPtr[1]);
                }
            }

            // Convert the byte ordering from RFC 4122 back to System.Guid
            // VaUuid uses big-endian for all fields
            // Guid uses little-endian for first three fields and big-endian for the rest
            public Guid ToGuid()
            {
                fixed (byte* ptr = data)
                {
                    byte[] bytes = new byte[16];
                    bytes[0] = ptr[3];
                    bytes[1] = ptr[2];
                    bytes[2] = ptr[1];
                    bytes[3] = ptr[0];
                    bytes[4] = ptr[5];
                    bytes[5] = ptr[4];
                    bytes[6] = ptr[7];
                    bytes[7] = ptr[6];
                    bytes[8] = ptr[8];
                    bytes[9] = ptr[9];
                    bytes[10] = ptr[10];
                    bytes[11] = ptr[11];
                    bytes[12] = ptr[12];
                    bytes[13] = ptr[13];
                    bytes[14] = ptr[14];
                    bytes[15] = ptr[15];
                    return new Guid(bytes);
                }
            }

            // Convert the byte ordering from System.Guid to RFC 4122
            // Guid uses little-endian for first three fields and big-endian for the rest
            // VaUuid uses big-endian for all fields
            public static UuidData FromGuid(Guid g)
            {
                byte[] bytes = g.ToByteArray();
                UuidData result = default;
                result.data[0] = bytes[3];
                result.data[1] = bytes[2];
                result.data[2] = bytes[1];
                result.data[3] = bytes[0];
                result.data[4] = bytes[5];
                result.data[5] = bytes[4];
                result.data[6] = bytes[7];
                result.data[7] = bytes[6];
                result.data[8] = bytes[8];
                result.data[9] = bytes[9];
                result.data[10] = bytes[10];
                result.data[11] = bytes[11];
                result.data[12] = bytes[12];
                result.data[13] = bytes[13];
                result.data[14] = bytes[14];
                result.data[15] = bytes[15];
                return result;
            }
        }
    }
}
