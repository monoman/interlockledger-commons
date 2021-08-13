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

namespace System.Buffers
{
    public class ReadOnlySequenceStream : Stream
    {
        public ReadOnlySequenceStream(ReadOnlyMemory<byte> memory) : this(new ReadOnlySequence<byte>(memory)) { }

        public ReadOnlySequenceStream(ReadOnlySequence<byte> memory) {
            _memory = memory;
            _position = 0;
            if (memory.Length > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(memory));
            _length = (int)memory.Length;
            if (_length > 0) {
                foreach (var segment in _memory.Slice(0, 1)) {
                    if (segment.IsEmpty)
                        continue;
                    FirstByte = segment.Span[0];
                    break;
                }
            }
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public byte? FirstByte { get; }
        public override long Length => _length;

        public override long Position {
            get => _position;
            set => SetPosition(value);
        }

        public override void Flush() {
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (_position + count > _length) count = _length - _position;
            var slice = _memory.Slice(_position, count);
            foreach (var segment in slice) {
                var buf = new Memory<byte>(buffer, offset, segment.Length);
                segment.CopyTo(buf);
                offset += segment.Length;
            }
            _position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin) => origin switch {
            SeekOrigin.Begin => SetPosition(offset),
            SeekOrigin.Current => SetPosition(_position + offset),
            SeekOrigin.End => SetPosition(_length + offset),
            _ => throw new InvalidOperationException()
        };

        public override void SetLength(long value) => throw new InvalidOperationException();

        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

        private readonly int _length;
        private readonly ReadOnlySequence<byte> _memory;
        private int _position;

        private long SetPosition(long value) {
            if (value >= 0 && value <= _length)
                _position = (int)value;
            return _position;
        }
    }
}