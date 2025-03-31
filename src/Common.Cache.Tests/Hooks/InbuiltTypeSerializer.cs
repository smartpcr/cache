// -----------------------------------------------------------------------
// <copyright file="InbuiltTypeSerializer.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Hooks
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class SimpleSerializer
    {
        private readonly JsonSerializerOptions options;

        public SimpleSerializer(JsonSerializerOptions? options = null)
        {
            this.options = options ?? new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultBufferSize = 1024,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                }
            };
        }

        public static SimpleSerializer Instance { get; } = new();

        public void Serialize<T>(T value, IBufferWriter<byte> target) where T : class, new()
        {
            var str = JsonHelper.SerializeObject(value, this.options);
            var length = Encoding.UTF8.GetByteCount(str);
            var buffer = target.GetMemory(length);
            var bytes = MemoryMarshal.AsBytes(str.AsSpan());
            Debug.Assert(bytes.Length == length);
            bytes.CopyTo(buffer.Span);
        }

        public T Deserialize<T>(ReadOnlySequence<byte> source) where T : class, new()
        {
            var length = checked((int)source.Length);
            var overSized = ArrayPool<byte>.Shared.Rent(length);
            source.CopyTo(overSized);
            var json = Encoding.UTF8.GetString(overSized, 0, length);
            ArrayPool<byte>.Shared.Return(overSized);
            return JsonHelper.DeserializeObject<T>(json, this.options);
        }
    }

    public static class JsonHelper
    {
        public static string SerializeObject<T>(T obj, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize(obj, options);
        }

        public static T DeserializeObject<T>(string json, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize<T>(json, options)!;
        }
    }
}