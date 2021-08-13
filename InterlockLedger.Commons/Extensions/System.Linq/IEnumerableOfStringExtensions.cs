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

namespace System.Linq
{
    public static class IEnumerableOfStringExtensions
    {
        public static bool AnyNullOrWhiteSpace(this IEnumerable<string> strings)
            => strings.SafeAny(kn => string.IsNullOrWhiteSpace(kn));

        public static IEnumerable<ulong> AsDistinctUlongs(this IEnumerable<string> strings)
            => strings.Safe().Select(StringExtensions.AsUlong).Distinct();

        public static IEnumerable<ulong> AsOrderedUlongs(this IEnumerable<string> strings)
            => strings.Safe().Select(StringExtensions.AsUlong);

        public static IEnumerable<string> SkipBlanks(this IEnumerable<string> strings)
            => strings.Skip(item => item.IsBlank());

        public static IEnumerable<string> SkipNonBlanks(this IEnumerable<string> strings)
            => strings.Skip(item => !item.IsBlank());

        public static IEnumerable<string> Trimmed(this IEnumerable<string> strings)
            => strings.SkipBlanks().Select(kn => kn.Trim());

        public static IEnumerable<string> Trimmed(this IEnumerable<string> strings, char trimChar)
            => strings.SkipBlanks().Select(kn => kn.Trim(trimChar));

        public static IEnumerable<string> Trimmed(this IEnumerable<string> strings, params char[] trimChars)
            => strings.SkipBlanks().Select(kn => kn.Trim(trimChars));
    }
}