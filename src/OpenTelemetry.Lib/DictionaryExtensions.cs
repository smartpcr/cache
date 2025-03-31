// -----------------------------------------------------------------------
// <copyright file="DictionaryExtensions.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenTelemetry.Lib;

/// <summary>
/// Extension methods on dictionary.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Get value by key in dictionary, return default value if not found.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <typeparam name="TKey">Type of key.</typeparam>
    /// <typeparam name="TValue">Type of value.</typeparam>
    /// <returns>The found value of default value.</returns>
    /// <exception cref="ArgumentNullException">The exception when dictionary is null.</exception>
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        where TKey : notnull
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
