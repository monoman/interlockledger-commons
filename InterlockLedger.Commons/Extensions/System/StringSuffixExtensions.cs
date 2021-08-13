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

namespace System;

public static class StringSuffixExtensions
{
    public static FileInfo TempFileInfo(this string suffix)
        => new(Path.GetTempFileName().WithSuffixReplaced(suffix));

    public static string WithSuffix(this string s, string suffix, char separator = '.')
        => SuffixAdder(s, suffix, separator, AddMissingSuffix);

    public static string WithSuffixReplaced(this string s, string suffix, char separator = '.')
        => SuffixAdder(s, suffix, separator, ReplaceSuffix);

    private static string AddMissingSuffix(string s, string suffix)
        => s.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ? s : s + suffix;

    private static string ReplaceSuffix(string s, string suffix) {
        int lastSeparatorPosition = s.LastIndexOf(suffix.First());
        return (lastSeparatorPosition > 1 ? s.Substring(0, lastSeparatorPosition) : s) + suffix;
    }

    private static string SuffixAdder(string? s, string suffix, char separator, Func<string, string, string> modifier) {
        return s is null
            ? string.Empty
            : string.IsNullOrWhiteSpace(suffix)
                ? TrimSeparator(s, separator)
                : modifier(TrimSeparator(s, separator), NormalizeSuffix(suffix, separator));
        static string NormalizeSuffix(string suffix, char separator)
            => separator + suffix.Trim().TrimStart(separator);
        static string TrimSeparator(string s, char separator) => s.Trim().TrimEnd(separator);
    }
}
