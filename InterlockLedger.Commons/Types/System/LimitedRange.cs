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

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace System;

[TypeConverter(typeof(TypeCustomConverter<LimitedRange>))]
[JsonConverter(typeof(JsonCustomConverter<LimitedRange>))]
public struct LimitedRange : IEquatable<LimitedRange>, ITextual<LimitedRange>
{
    public readonly ulong End;
    public readonly ulong Start;

    public LimitedRange(ulong start) : this(start, 1) {
    }

    public LimitedRange(ulong start, ushort count) {
        if (count == 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        Start = start;
        checked {
            End = start + count - 1;
        }
        TextualRepresentation = $"[{Start}{(End != Start ? "-" + End : "")}]";
    }

    public LimitedRange(string textualRepresentation) {
        var parts = textualRepresentation.Required(nameof(textualRepresentation)).Trim('[', ']').Split('-');
        string startText = parts[0].Trim();
        Start = ulong.Parse(startText, CultureInfo.InvariantCulture);
        if (parts.Length == 1) {
            End = Start;
        } else {
            string endText = parts[1].Trim();
            End = ulong.Parse(endText, CultureInfo.InvariantCulture);
            if (End < Start)
                throw new ArgumentException($"End of range ['{endText}' => {End}] must be greater than the start ['{startText}' => {Start}]", nameof(textualRepresentation));
            if (End > (Start + ushort.MaxValue))
                throw new ArgumentException($"Range is too wide (Count > {ushort.MaxValue}");
        }
        TextualRepresentation = $"[{Start}{(End != Start ? "-" + End : "")}]";
    }

    public static ITextualService<LimitedRange> TextualService { get; } = new LimitedRangeTextualService();
    public ushort Count => (ushort)(End - Start + 1);
    public bool IsEmpty => Start == End && End == default;
    public bool IsInvalid => Start > End;
    public string TextualRepresentation { get; }

    public static bool operator !=(LimitedRange left, LimitedRange right) => !(left == right);

    public static bool operator ==(LimitedRange left, LimitedRange right) => left.Equals(right);

    public bool Contains(ulong value) => Start <= value && value <= End;

    public bool Contains(LimitedRange other) => Contains(other.Start) && Contains(other.End);

    public override bool Equals(object? obj) => obj is LimitedRange limitedRange && Equals(limitedRange);

    public bool Equals(LimitedRange other) => End == other.End && Start == other.Start;

    public override int GetHashCode() => HashCode.Combine(End, Start);

    public bool OverlapsWith(LimitedRange other) => Contains(other.Start) || Contains(other.End) || other.Contains(Start);

    public override string ToString() => TextualRepresentation;

    private LimitedRange(ulong start, ulong end, string textualRepresentation) {
        Start = start;
        End = end;
        TextualRepresentation = textualRepresentation;
    }

    private class LimitedRangeTextualService : ITextualService<LimitedRange>
    {
        public LimitedRange Empty { get; } = new LimitedRange(start: default,
                                                              end: default,
                                                              textualRepresentation: "[]");
        public LimitedRange Invalid { get; } = new LimitedRange(start: ulong.MaxValue,
                                                                end: ulong.MaxValue - 1,
                                                                textualRepresentation: "[?]");
        public Regex Mask { get; } = new Regex(@"^\[\d+(-\d+)?\]$");
        public string MessageForMissing => "No LimitedRange supplied";

        public LimitedRange Build(string textualRepresentation) {
            if (textualRepresentation.IsBlank() || textualRepresentation.SafeEqualsTo(Empty.TextualRepresentation))
                return Empty;
            try {
                return new LimitedRange(textualRepresentation);
            } catch {
                return Invalid;
            }
        }

        public string MessageForInvalid(string? textualRepresentation) => $"Not a valid LimitedRange '{textualRepresentation}'";
    }
}

public static class IEnumerableOfLimitedRangeExtensions
{
    public static bool AnyOverlapsWith(this IEnumerable<LimitedRange> first, IEnumerable<LimitedRange> second) => first.Any(f => second.Any(s => s.OverlapsWith(f)));

    public static bool Includes(this IEnumerable<LimitedRange> ranges, ulong value) => ranges.Any(r => r.Contains(value));

    public static bool IsSupersetOf(this IEnumerable<LimitedRange> first, IEnumerable<LimitedRange> second) => second.All(r => first.Any(Value => Value.Contains(r)));
}
