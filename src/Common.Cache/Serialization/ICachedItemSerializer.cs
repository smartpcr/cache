﻿// -----------------------------------------------------------------------
// <copyright file="ICachedItemSerializer.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Serialization
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Hybrid;

    public interface ICachedItemSerializer<T> : IHybridCacheSerializer<T>
    {
        /// <summary>
        /// Serialized the specified <paramref name="obj"/> into a byte[].
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="obj"/> parameter.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The byte[] which represents the serialized <paramref name="obj"/>.</returns>
        byte[] Serialize(T obj);

        /// <summary>
        /// Deserialized the specified byte[] <paramref name="data"/> into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to be returned.</typeparam>
        /// <param name="data">The data to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize(byte[] data);

        /// <summary>
        /// Serialized the specified <paramref name="obj"/> into a byte[].
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="obj"/> parameter.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The byte[] which represents the serialized <paramref name="obj"/>.</returns>
        ValueTask<byte[]> SerializeAsync(T obj, CancellationToken token = default);

        /// <summary>
        /// Deserialized the specified byte[] <paramref name="data"/> into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to be returned.</typeparam>
        /// <param name="data">The data to deserialize.</param>
        /// /// <param name="token">The cancellation token.</param>
        /// <returns>The deserialized object.</returns>
        ValueTask<T> DeserializeAsync(byte[] data, CancellationToken token = default);
    }
}