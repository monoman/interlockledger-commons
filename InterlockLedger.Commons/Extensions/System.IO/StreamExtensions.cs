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

using System.Buffers;

namespace System.IO
{
    public static partial class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, long fileSizeLimit, int bufferSize, CancellationToken cancellationToken) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));
            if (source.CanSeek)
                CheckSizeLimit(source.Length);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try {
                long totalBytes = 0;
                while (true) {
                    int bytesRead = await source.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0) break;
                    totalBytes += bytesRead;
                    CheckSizeLimit(totalBytes);
                    await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            void CheckSizeLimit(long totalBytes) {
                if (fileSizeLimit > 0 && totalBytes > fileSizeLimit)
                    throw new InvalidDataException($"Stream is too big to copy (> {fileSizeLimit})");
            }
        }

        public static bool HasBytes(this Stream s) => !(s is null) && s.CanSeek && s.Position < s.Length;

        public static async Task<byte[]> ReadAllBytesAsync(this Stream s) {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            using var buffer = new MemoryStream();
            await s.CopyToAsync(buffer).ConfigureAwait(false);
            return buffer.ToArray();
        }

        public static byte[] ReadBytes(this Stream s, int length) {
            if (s is null || length <= 0)
                return Array.Empty<byte>();
            var bytes = new byte[length];
            var offset = 0;
            var retries = 3;
            while (length > 0) {
                var count = s.Read(bytes, offset, length);
                if (count == length)
                    break;
                if (count == 0) {
                    if (retries-- < 1)
                        throw new TooFewBytesException();
                    Thread.Sleep(100);
                } else
                    retries = 3;
                length -= count;
                offset += count;
            }
            return bytes;
        }

        public static byte[] ReadExactly(this Stream s, int length) {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            var offset = 0;
            var buffer = new byte[length];
            while (offset < length) {
                offset += s.Read(buffer, offset, length - offset);
            }
            return buffer;
        }

        public static byte ReadSingleByte(this Stream s) {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            var bytes = new byte[1];
            var retries = 3;
            while (retries-- > 0) {
                if (s.Read(bytes, 0, 1) == 1)
                    return bytes[0];
                Thread.Sleep(100);
            }
            throw new TooFewBytesException();
        }

        public static Stream WriteBytes(this Stream s, byte[] bytes) {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            if (bytes?.Length > 0)
                s.Write(bytes, 0, bytes.Length);
            s.Flush();
            return s;
        }

        public static Stream WriteSingleByte(this Stream s, byte value) {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            s.WriteByte(value);
            return s;
        }

        private static byte AsByte(long value) => (byte)(value & 0xFF);

        private static byte AsByteU(ulong value) => (byte)(value & 0xFF);

        private static byte ReadByte(Stream s) {
            var retries = 3;
            while (retries-- > 0) {
                var v = s.ReadByte();
                if (v >= 0)
                    return (byte)v;
                Thread.Sleep(100);
            }
            throw new TooFewBytesException();
        }
    }
}