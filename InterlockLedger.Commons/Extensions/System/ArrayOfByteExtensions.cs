// ******************************************************************************************************************************
//
// Copyright (c) 2018-2021 InterlockLedger Network
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of the copyright holder nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES, LOSS OF USE, DATA, OR PROFITS, OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// ******************************************************************************************************************************

using System.Security.Cryptography.X509Certificates;

namespace System;

public static class ArrayOfByteExtensions
{
    public static byte[] Append(this byte[] bytes, byte[] newBytes) {
        if (newBytes is null)
            return bytes;
        if (bytes is null)
            return newBytes;
        var result = new byte[bytes.Length + newBytes.Length];
        Array.Copy(bytes, result, bytes.Length);
        Array.Copy(newBytes, 0, result, bytes.Length, newBytes.Length);
        return result;
    }

    public static string AsLiteral(this byte[] bytes) => $"new byte[] {{ {bytes.JoinedBy(", ")} }}";

    public static long AsLong(this byte[] bytes, int offset = 0) {
        if ((SafeLength(bytes) - offset) < 8)
            throw new ArgumentException("Must have 8 bytes to convert to long", nameof(bytes));
        var part = PartOf(bytes, 8, offset);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(part);
        return BitConverter.ToInt64(part, 0);
    }

    public static ulong AsULong(this byte[] bytes, int offset = 0) {
        if ((SafeLength(bytes) - offset) < 8)
            throw new ArgumentException("Must have 8 bytes to convert to ulong", nameof(bytes));
        var part = PartOf(bytes, 8, offset);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(part);
        return BitConverter.ToUInt64(part, 0);
    }

    public static string AsUTF8String(this byte[] bytes) => Encoding.UTF8.GetString(bytes);

    public static string Chunked(this byte[] bytes, int length) {
        if (bytes == null) {
            throw new ArgumentNullException(nameof(bytes));
        }
        length = (int)(Math.Floor((decimal)(Math.Abs(length) / 4)) + 1) * 4;
        var value = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
        var sb = new StringBuilder();
        var start = 0;
        while (start < value.Length) {
            var howMany = Math.Min(length, value.Length - start);
            sb.Append(value, start, howMany).Append(Environment.NewLine);
            start += length;
        }
        return sb.ToString();
    }

    public static int CompareTo(this byte[]? bytes1, byte[]? bytes2) {
        var length1 = bytes1?.Length ?? 0;
        var length2 = bytes2?.Length ?? 0;
        var lengthToCompare = Math.Min(length1, length2);
        for (var i = 0; i < lengthToCompare; i++) {
            byte b1 = bytes1![i];
            byte b2 = bytes2![i];
            if (b1 > b2)
                return 1;
            if (b1 < b2)
                return -1;
        }
        return length1 > lengthToCompare ? 1 : length2 > lengthToCompare ? -1 : 0;
    }

    public static void DumpToFile(this byte[] bytes, string filename) {
        if (bytes is null)
            throw new ArgumentNullException(nameof(bytes));
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException("message", nameof(filename));
        using var file = File.CreateText(filename);
        var count = 0;
        foreach (var b in bytes) {
            file.Write($"{b:X2} ");
            if (++count >= 16) {
                count = 0;
                file.WriteLine();
            }
        }
        file.WriteLine();
        file.Flush();
    }

    public static byte[] FromSafeBase64(this string base64) {
        if (!string.IsNullOrWhiteSpace(base64)) {
            string normalized = PadBase64(base64.Trim()).Replace('-', '+').Replace('_', '/');
            try {
                return Convert.FromBase64String(normalized);
            } catch (Exception e) {
                Console.WriteLine($"Error converting {normalized}");
                Console.WriteLine(e);
            }
        }
        return Array.Empty<byte>();
    }

    public static bool HasSameBytesAs(this byte[] bytes1, params byte[] bytes2) {
        if (bytes1 == null || bytes2 == null)
            return false;
        if (bytes1.Length != bytes2.Length)
            return false;
        for (var i = 0; i < bytes1.Length; i++) {
            if (bytes1[i] != bytes2[i])
                return false;
        }
        return true;
    }

    public static X509Certificate2 OpenCertificate(this byte[] certificateBytes, string password)
        => new(certificateBytes, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

    public static byte[] PartOf(this byte[] bytes, int length, int offset = 0) {
        var part = new byte[length];
        Array.Copy(bytes, offset, part, 0, length);
        return part;
    }

    public static int SafeGetHashCode(this byte[] bytes) => bytes?.ToSafeBase64().GetHashCode(StringComparison.InvariantCulture) ?? 0;

    public static int SafeLength(this byte[] bytes) => bytes?.Length ?? 0;

    public static string ToSafeBase64(this IEnumerable<byte> bytes)
        => ToSafeBase64(bytes.ToArray());

    public static string ToSafeBase64(this byte[] bytes)
        => Convert.ToBase64String(bytes.Required(nameof(bytes))).Trim('=').Replace('+', '-').Replace('/', '_');

    public static string ToSafeBase64(this ReadOnlyMemory<byte> readOnlyBytes) // TODO try to not create an Array
        => ToSafeBase64(readOnlyBytes.ToArray());

    public static string ToSafeBase64(this IEnumerable<ReadOnlyMemory<byte>> readOnlyBytes)
        => readOnlyBytes == null ? string.Empty : readOnlyBytes.Select(s => ToSafeBase64(s)).JoinedBy("");

    private static string PadBase64(string base64) {
        while (base64.Length % 4 != 0)
            base64 += "=";
        return base64;
    }
}
