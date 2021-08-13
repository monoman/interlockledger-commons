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

namespace System.Collections.Generic.Tests;

[TestFixture]
public class SingleEnumerableTests
{
    [Test]
    public void SingleEnumerableTest() {
        var single = new SingleEnumerable<int>(42);
        Assert.NotNull(single);
        Assert.AreEqual(42, single.First());
        Assert.AreEqual(42, single.Last());
        Assert.AreEqual(1, single.Count());
        var enumerator = single.GetEnumerator();
        Assert.NotNull(enumerator);
        Assert.IsInstanceOf<IEnumerator<int>>(enumerator);
        Assert.AreEqual(0, enumerator.Current);
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual(42, enumerator.Current);
        Assert.IsFalse(enumerator.MoveNext());
        Assert.AreEqual(0, enumerator.Current);
        enumerator.Reset();
        Assert.AreEqual(0, enumerator.Current);
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual(42, enumerator.Current);
        Assert.IsFalse(enumerator.MoveNext());
        Assert.AreEqual(0, enumerator.Current);
    }
}
