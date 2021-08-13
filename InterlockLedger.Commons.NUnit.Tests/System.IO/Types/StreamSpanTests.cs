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

using NUnit.Framework;

namespace System.IO;

public class StreamSpanTests
{
    [Test]
    public void TestSkippingToEndOfSpanOnDisposeWithoutSeek() {
        using var baseStream = new NonSeekMemoryStream(new byte[100]);
        TestSkippingOn(baseStream);
    }

    [Test]
    public void TestSkippingToEndOfSpanOnDisposeWithSeek() {
        using var baseStream = new MemoryStream(new byte[100]);
        TestSkippingOn(baseStream);
    }

    private static void TestSkippingOn(Stream baseStream) {
        baseStream.Seek(10, SeekOrigin.Begin);
        Assert.AreEqual(10L, baseStream.Position);
        baseStream.WriteByte(30);
        baseStream.Seek(10, SeekOrigin.Begin);
        Assert.AreEqual(10L, baseStream.Position);
        using (var sp = new StreamSpan(baseStream, (ulong)baseStream.ReadByte())) {
            Assert.AreEqual(30L, sp.Length);
            Assert.AreEqual(0L, sp.Position);
            Assert.AreEqual(11L, baseStream.Position);
            _ = sp.ReadBytes(20);
            Assert.AreEqual(20L, sp.Position);
            Assert.AreEqual(31L, baseStream.Position);
            if (sp.CanSeek) {
                sp.Position = 30;
                Assert.AreEqual(41L, baseStream.Position);
                sp.Position = 5;
                Assert.AreEqual(16L, baseStream.Position);
                sp.Seek(30, SeekOrigin.Begin);
                Assert.AreEqual(41L, baseStream.Position);
                sp.Position = 5;
                Assert.AreEqual(16L, baseStream.Position);
                sp.Seek(0, SeekOrigin.End);
                Assert.AreEqual(41L, baseStream.Position);
                sp.Position = 5;
                Assert.AreEqual(16L, baseStream.Position);
                sp.Seek(25, SeekOrigin.Current);
                Assert.AreEqual(41L, baseStream.Position);
                sp.Position = 5;
                Assert.AreEqual(16L, baseStream.Position);
            }
        }
        Assert.AreEqual(41L, baseStream.Position);
        using (var sp2 = new StreamSpan(baseStream, (ulong)(baseStream.Length - baseStream.Position))) {
            Assert.AreEqual(0L, sp2.Position);
            Assert.AreEqual(41L, baseStream.Position);
            _ = sp2.ReadBytes(20);
            Assert.AreEqual(20L, sp2.Position);
            Assert.AreEqual(61L, baseStream.Position);
        }
        Assert.AreEqual(baseStream.Length, baseStream.Position);
    }

    private class NonSeekMemoryStream : MemoryStream
    {
        public NonSeekMemoryStream(byte[] buffer) : base(buffer, writable: true) {
        }

        public override bool CanSeek => false;
    }
}
