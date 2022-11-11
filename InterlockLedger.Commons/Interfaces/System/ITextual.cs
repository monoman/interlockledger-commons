// ******************************************************************************************************************************
//  
// Copyright (c) 2018-2022 InterlockLedger Network
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

public interface ITextual
{
    bool IsEmpty { get; }
    bool IsInvalid { get; }
    string TextualRepresentation { get; }
}

public interface ITextual<T> : ITextual, IParsable<T> where T : ITextual<T>
{
    protected static abstract T FromString(string textualRepresentation);
    protected static abstract string MessageForInvalid(string? textualRepresentation);

    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out T result) {
        result = Resolve(s!);
        return !result.IsInvalid;
    }

    public enum Resolution
    {
        Empty,
        Valid,
        Invalid
    }

    public static abstract T Empty { get; }
    public static abstract T Invalid { get; }
    protected static abstract Regex Mask { get; }
    protected static abstract string MessageForMissing { get; }

    public static Resolution IsValidTextual(string? textualRepresentation) =>
         textualRepresentation.IsBlank() || textualRepresentation.SafeEqualsTo(T.Empty.TextualRepresentation)
            ? Resolution.Empty
            : T.Mask.IsMatch(textualRepresentation)
                ? Resolution.Valid
                : Resolution.Invalid;

    public static T Resolve(string textualRepresentation) =>
        IsValidTextual(textualRepresentation) switch {
            Resolution.Empty => T.Empty,
            Resolution.Valid => T.FromString(textualRepresentation),
            _ => T.Invalid
        };

    public static string? Validate(string? textualRepresentation) =>
        IsValidTextual(textualRepresentation) switch {
            Resolution.Valid => null,
            Resolution.Empty => T.MessageForMissing,
            _ => T.MessageForInvalid(textualRepresentation)
        };
}
