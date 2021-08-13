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

public static class ObjectExtensions
{
    public static IEnumerable<T?> AsSingle<T>(this T s) => new SingleEnumerable<T>(s);

    public static List<T?> AsSingleList<T>(this T s) => s.AsSingle().ToList();

    public static async Task<TO?> IfNotNullAsync<T, TO>(this T? value, Func<T, Task<TO?>> transformAsync) where T : class where TO : class
        => transformAsync is null
            ? throw new ArgumentNullException(nameof(transformAsync))
            : value is null ? null : await transformAsync(value).ConfigureAwait(false);

    public static bool In<T>(this T value, params T[] set) => set?.Contains(value) ?? false;

    public static bool In<T>(this T value, IEnumerable<T> set) => set?.Contains(value) ?? false;

    public static bool IsDefault<T>(this T value) => value is null || value.Equals(default(T));

    public static string? PadLeft(this object? value, int totalWidth) => value?.ToString()?.PadLeft(totalWidth);

    public static string? PadRight(this object? value, int totalWidth) => value?.ToString()?.PadRight(totalWidth);

    public static T Required<T>([NotNull] this T? value, string name) where T : class
        => value ?? throw new ArgumentException("Required", name);

    public static string TypeOrNull(this object? value) => value is null ? "null" : value.GetType().Name;

    public static string WithDefault(this object? value, string @default)
        => StringExtensions.WithDefault(value?.ToString(), @default);
}
