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

namespace System.Linq;

public static class IEnumerableOfTExtensions
{
    public static bool AnyWithNoNulls<T>(this IEnumerable<T> items) => items.SafeAny() && items.NoNulls();

    public static IEnumerable<T> Append<T>(this IEnumerable<T> first, params T[] extras)
        => first.Safe().Concat(extras);

    public static IEnumerable<T> Append<T>(this IEnumerable<T> first, IEnumerable<T> second)
        => first.Safe().Concat(second);

    public static IEnumerable<T> AppendedOf<T>(this T item, IEnumerable<T> remainingItems)
        => InnerAppend(item, remainingItems);

    public static IEnumerable<T> AppendedOf<T>(this T item, params T[] remainingItems)
        => InnerAppend(item, remainingItems);

    public static string? DefaultFormatter<T>(T? t) => t?.ToString();

    public static bool EqualTo<T>(this IEnumerable<T> first, IEnumerable<T> second)
        => first is null && second is null || !(first is null || second is null) && first.SequenceEqual(second);

    public static bool EquivalentTo<T>(this IEnumerable<T> first, IEnumerable<T> second)
        => first.Safe().SequenceEqual(second.Safe());

    public static IEnumerable<T> ExceptFor<T>(this IEnumerable<T> first, params T[] exceptions)
        => first.Safe().Except(exceptions);

    public static IEnumerable<T?> FilledTo<T>(this IEnumerable<T?> list, int length) {
        if (list != null)
            foreach (var item in list) {
                if (length == 0)
                    yield break;
                length--;
                yield return item;
            }
        while (length-- > 0)
            yield return default;
    }

    public static IEnumerable<T> IfAnyDo<T>(this IEnumerable<T> values, Action action) {
        if (values.SafeAny())
            action();
        return values;
    }

    public static string JoinedBy<T>(this IEnumerable<T> list, string joiner)
        => list == null ? string.Empty : string.Join(joiner, list);

    public static string JoinedBy<T>(this IEnumerable<T> list, string joiner, Func<T?, string?> formatter)
        => list == null ? string.Empty : string.Join(joiner, list.Select(formatter ?? DefaultFormatter));

    public static bool None<T>(this IEnumerable<T> items) => !items.SafeAny();

    public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate) => !items.SafeAny(predicate);

    public static IEnumerable<T> NonEmpty<T>(this IEnumerable<T> items, string parameterName)
        => items.SafeAny() ? items : throw new ArgumentException("Should not be empty", parameterName);

    public static bool NoNulls<T>(this IEnumerable<T> items) => items.None(item => item is null);

    public static IEnumerable<T>? NullIfEmpty<T>(this IEnumerable<T>? items)
        => items.SafeAny() ? items : null;

    public static IEnumerable<T> PrependedBy<T>(this T item, IEnumerable<T> prefixItems) {
        foreach (var it in prefixItems.Safe())
            yield return it;
        yield return item;
    }

    public static IEnumerable<T> PrependedBy<T>(this IEnumerable<T> items, IEnumerable<T> prefixItems) {
        foreach (var it in prefixItems.Safe())
            yield return it;
        foreach (var it in items.Safe())
            yield return it;
    }

    public static IEnumerable<T> PrependedBy<T>(this IEnumerable<T> items, params T[] prefixItems) {
        foreach (var it in prefixItems.Safe())
            yield return it;
        foreach (var it in items.Safe())
            yield return it;
    }

    public static IEnumerable<T> RequireNonNulls<T>(this IEnumerable<T> items, string parameterName) where T : class
        => items.None(item => item is null)
            ? items.Safe()
            : throw new ArgumentException("List must not contain null elements", parameterName);

    public static IEnumerable<T> Safe<T>(this IEnumerable<T>? values) => values ?? Enumerable.Empty<T>();

    public static bool SafeAny<T>(this IEnumerable<T>? values) => values?.Any() ?? false;

    public static bool SafeAny<T>(this IEnumerable<T>? items, Func<T, bool> predicate) => items?.Any(predicate) == true;

    public static IEnumerable<T> SafeConcat<T>(this IEnumerable<T> items, IEnumerable<T> remainingItems)
        => InnerConcat(Safe(items), remainingItems);

    public static int SafeCount<T>(this IEnumerable<T> values) => values?.Count() ?? -1;

    public static IEnumerable<TResult> SelectSkippingNulls<TSource, TResult>(this IEnumerable<TSource>? values, Func<TSource?, TResult?> selector) where TResult : class
        => values.Safe().Select(selector).SkipNulls();

    public static IEnumerable<T> Skip<T>(this IEnumerable<T> values, Func<T, bool> predicate)
        => values.Safe().Where(item => !predicate(item));

    public static IEnumerable<T> SkipDefaults<T>(this IEnumerable<T> values) => values.Skip(item => item.IsDefault());

    public static IEnumerable<T> SkipNonDefaults<T>(this IEnumerable<T> values) => values.Skip(item => !item.IsDefault());

    public static IEnumerable<T> SkipNonNulls<T>(this IEnumerable<T> values) where T : class
        => values.Skip(item => item is not null);

    public static IEnumerable<T> SkipNulls<T>(this IEnumerable<T?> values) where T : class
        => values.Skip(item => item is null)!;

    public static IEnumerable<T> TakeULong<T>(this IEnumerable<T> items, ulong howMany)
        => (howMany <= int.MaxValue) ? items.Safe().Take((int)howMany) : items.Safe();

    public static IEnumerable<string> ToStrings<T>(this IEnumerable<T> values)
        => values.Safe().Select(v => v.WithDefault(string.Empty));

    public static string ValidateAll<T>(this IEnumerable<T> items, Func<T, string> validator)
        => items.Safe().Select(validator.Required(nameof(validator))).SkipBlanks().WithLineBreaks();

    public static string WithCommas<T>(this IEnumerable<T> list, bool noSpaces = false)
        => JoinedBy(list, noSpaces ? "," : ", ");

    public static IEnumerable<T> WithDefault<T>(this IEnumerable<T> values, Func<IEnumerable<T>?>? alternativeValues)
        => values.SafeAny() ? values : Safe(alternativeValues?.Invoke());

    public static IEnumerable<T> WithDefault<T>(this IEnumerable<T> values) => Safe(values);

    public static IEnumerable<T> WithDefault<T>(this IEnumerable<T> values, params T[] alternativeValues)
        => WithDefault(values, (IEnumerable<T>)alternativeValues);

    public static IEnumerable<T> WithDefault<T>(this IEnumerable<T> values, IEnumerable<T> alternativeValues)
        => values.SafeAny() ? values : Safe(alternativeValues);

    public static async Task<IEnumerable<T>> WithDefaultAsync<T>(
        this Task<IEnumerable<T>> getValuesAsync,
        Func<Task<IEnumerable<T>>> getAlternativeValuesAsync) where T : class
        => await WithDefaultAsync(getValuesAsync is not null
                                        ? (await getValuesAsync)
                                        : Enumerable.Empty<T>(),
                                  getAlternativeValuesAsync);

    public static async Task<IEnumerable<T>> WithDefaultAsync<T>(
        this IEnumerable<T> values,
        Func<Task<IEnumerable<T>>> getAlternativeValuesAsync) where T : class {
        var nonNullValues = values.SkipNulls();
        return nonNullValues.Any()
            ? nonNullValues
            : (await getAlternativeValuesAsync.Required(nameof(getAlternativeValuesAsync))()).Safe();
    }

    public static string WithLineBreaks<T>(this IEnumerable<T> list) => JoinedBy(list, "\n");

    public static IEnumerable<T?> WithMinimums<T>(this IEnumerable<T?> values, params T[] minimums)
        where T : IComparable<T>
        => minimums.Zip(values.FilledTo(minimums.Length), (min, p) => min.CompareTo(p) >= 0 ? min : p);

    private static IEnumerable<T> InnerAppend<T>(T item, IEnumerable<T> remainingItems)
        => new SingleEnumerable<T>(item)!.SafeConcat(remainingItems);

    private static IEnumerable<T> InnerConcat<T>(IEnumerable<T> items, IEnumerable<T> remainingItems)
        => remainingItems.SafeAny() ? items.Concat(remainingItems) : items;
}
