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

namespace System.IO
{
    public class StreamSpan : Stream
    {
        public StreamSpan(Stream s, ulong length) : this(s, -1, length) {
        }

        public StreamSpan(Stream s, long offset, ulong length, bool closeWrappedStreamOnDispose = false) {
            _s = s.Required(nameof(s));
            _closeWrappedStreamOnDispose = closeWrappedStreamOnDispose;
            if (!s.CanRead)
                throw new ArgumentException("original stream needs to be readable");
            _length = (long)length;
            if (_length < 0)
                throw new ArgumentException("length is too large and wrapped around!!!");
            if (offset >= 0 && offset != s.Position)
                s.Position = s.CanSeek
                    ? offset
                    : throw new ArgumentException("offset doesn't match current position on non-seeking stream");
            _begin = s.Position;
            _positionAfterSpan = _length + _begin;
        }

        public override bool CanRead => true;

        public override bool CanSeek => _s.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position {
            get => _s.Position - _begin;
            set {
                if (!CanSeek)
                    throw new NotSupportedException("Can't position non-seekable stream");
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Can't position before start");
                if (value > _length)
                    throw new ArgumentOutOfRangeException(nameof(value), "Can't position after end");
                _s.Position = (value + _begin);
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => throw new NotSupportedException(_isReadonly);

        public override void EndWrite(IAsyncResult asyncResult) => throw new NotSupportedException(_isReadonly);

        public override void Flush() {
        }

        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public override int Read(byte[] buffer, int offset, int count) {
            if (Position + count > _length) {
                var newCount = _length - Position;
                if (newCount <= 0)
                    return 0;
                count = newCount > int.MaxValue ? int.MaxValue : (int)newCount;
            }
            return _s.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return CanSeek
                ? _s.Seek(AdjustOffset(offset, origin), SeekOrigin.Begin) - _begin
                : throw new NotSupportedException("Can't position non-seekable stream");

            long AdjustOffset(long offset, SeekOrigin origin)
                => origin switch {
                    SeekOrigin.Begin => ValidateWithinBounds(offset),
                    SeekOrigin.Current => ValidateWithinBounds(offset + Position),
                    SeekOrigin.End => ValidateWithinBounds(_length + offset),
                    _ => throw new ArgumentException($"Unknown origin {origin}")
                };
            long ValidateWithinBounds(long offset)
                => offset < 0
                    ? throw new ArgumentOutOfRangeException(nameof(offset), "Can't position before start")
                    : offset > _length
                        ? throw new ArgumentOutOfRangeException(nameof(offset), "Can't position after end")
                        : offset + _begin;
        }

        public override void SetLength(long value) => throw new NotSupportedException("This StreamSpan can't have its length changed");

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException(_isReadonly);

        public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException(_isReadonly);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException(_isReadonly);

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException(_isReadonly);

        public override void WriteByte(byte value) => throw new NotSupportedException(_isReadonly);

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (_closeWrappedStreamOnDispose) {
                _s.Dispose();
            } else {
                var unreadBytes = (int)(_positionAfterSpan - _s.Position);
                if (unreadBytes > 0) {
                    if (CanSeek) {
                        _s.Position = _positionAfterSpan;
                    } else {
                        AdvanceByReading(_s, unreadBytes);
                    }
                }
            }

            static void AdvanceByReading(Stream s, int unreadBytes) {
                var bufferSize = Math.Min(unreadBytes, 16 * 1024);
                var buffer = new byte[unreadBytes];
                while (unreadBytes > bufferSize) {
                    var read = s.Read(buffer, 0, bufferSize);
                    if (read == 0)
                        return;
                    unreadBytes -= read;
                }
                s.Read(buffer, 0, unreadBytes);
            }
        }

        private const string _isReadonly = "This StreamSpan is readonly";
        private readonly long _begin;
        private readonly bool _closeWrappedStreamOnDispose;
        private readonly long _length;
        private readonly long _positionAfterSpan;
        private readonly Stream _s;
    }
}