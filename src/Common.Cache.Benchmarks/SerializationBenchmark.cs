// -----------------------------------------------------------------------
// <copyright file="SerializationBenchmark.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Benchmarks
{
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using Common.Cache.Serialization;
    using Newtonsoft.Json;
#if NET462 || NETSTANDARD2_0
    using MessagePack;
#else
    using MemoryPack;
#endif

    [MemoryDiagnoser]
    public class SerializationBenchmark
    {
        [Params(16, 16348, 5242880)]
        public int PayloadSize { get; set; }

        [Benchmark]
        public void SerializeWithNewtonsoftJson()
        {
            var customer = Customer.CreateTestData(this.PayloadSize);
            var json = JsonConvert.SerializeObject(customer);
            JsonConvert.DeserializeObject<Customer>(json);

            var products = Product.CreateTestData(this.PayloadSize);
            var json2 = JsonConvert.SerializeObject(products);
            JsonConvert.DeserializeObject<List<Product>>(json2);
        }

        [Benchmark]
        public void SerializeWithSystemTextJson()
        {
            var customer = Customer.CreateTestData(this.PayloadSize);
            var json = System.Text.Json.JsonSerializer.Serialize(customer);
            System.Text.Json.JsonSerializer.Deserialize<Customer>(json);

            var products = Product.CreateTestData(this.PayloadSize);
            var json2 = System.Text.Json.JsonSerializer.Serialize(products);
            System.Text.Json.JsonSerializer.Deserialize<List<Product>>(json2);
        }

        [Benchmark]
        public void SerializeWithMessagePack()
        {
            var customer = Customer.CreateTestData(this.PayloadSize);
            var products = Product.CreateTestData(this.PayloadSize);

#if NET462 || NETSTANDARD2_0
            var msgPackSerializerOptions =
                MessagePackSerializerOptions.Standard
                    .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
            var bytes = MessagePackSerializer.Serialize(customer, msgPackSerializerOptions);
            MessagePackSerializer.Deserialize<Customer>(bytes, msgPackSerializerOptions);

            var bytes2 = MessagePackSerializer.Serialize(products, msgPackSerializerOptions);
            MessagePackSerializer.Deserialize<List<Product>>(bytes2, msgPackSerializerOptions);
#else
            var buffer = new PoolBufferWriter();
            var memSerializationOptions = new MemoryPackSerializerOptions();
            MemoryPackWriter<PoolBufferWriter> writer = new(ref buffer, MemoryPackWriterOptionalStatePool.Rent(memSerializationOptions));
            MemoryPackSerializer.Serialize(ref writer, customer);
            var bytes = buffer.ToArray();
            MemoryPackSerializer.Deserialize<Customer>(bytes, memSerializationOptions);

            var buffer2 = new PoolBufferWriter();
            MemoryPackWriter<PoolBufferWriter> writer2 = new(ref buffer2, MemoryPackWriterOptionalStatePool.Rent(memSerializationOptions));
            MemoryPackSerializer.Serialize(ref writer2, products);
            var bytes2 = buffer.ToArray();
            MemoryPackSerializer.Deserialize<List<Product>>(bytes2, memSerializationOptions);
#endif
        }
    }
}