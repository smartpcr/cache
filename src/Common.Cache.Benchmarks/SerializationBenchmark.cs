// -----------------------------------------------------------------------
// <copyright file="SerializationBenchmark.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using Common.Cache.Serialization;
    using Common.Cache.Tests.Steps;
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
        }

        [Benchmark]
        public void SerializeWithSystemTextJson()
        {
            var customer = Customer.CreateTestData(this.PayloadSize);
            var json = System.Text.Json.JsonSerializer.Serialize(customer);
            System.Text.Json.JsonSerializer.Deserialize<Customer>(json);
        }

        [Benchmark]
        public void SerializeWithMessagePack()
        {
            var customer = Customer.CreateTestData(this.PayloadSize);
#if NET462 || NETSTANDARD2_0
            var msgPackSerializerOptions =
                MessagePackSerializerOptions.Standard
                    .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
            var bytes = MessagePackSerializer.Serialize(customer, msgPackSerializerOptions);
            MessagePackSerializer.Deserialize<Customer>(bytes, msgPackSerializerOptions);
#else
            var buffer = new PoolBufferWriter();
            var memSerializationOptions = new MemoryPackSerializerOptions();
            MemoryPackWriter<PoolBufferWriter> writer = new(ref buffer, MemoryPackWriterOptionalStatePool.Rent(memSerializationOptions));
            MemoryPackSerializer.Serialize(ref writer, customer);
            var bytes = buffer.ToArray();
            MemoryPackSerializer.Deserialize<Customer>(bytes, memSerializationOptions);
#endif
        }
    }
}