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
    using FluentAssertions;
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
            var deserializedCustomer = JsonConvert.DeserializeObject<Customer>(json);
            deserializedCustomer.Should().BeEquivalentTo(customer);
        }

        [Benchmark]
        public void SerializeWithSystemTextJson()
        {
            var customer = Customer.CreateTestData(this.PayloadSize);
            var json = System.Text.Json.JsonSerializer.Serialize(customer);
            var deserializedCustomer = System.Text.Json.JsonSerializer.Deserialize<Customer>(json);
            deserializedCustomer.Should().BeEquivalentTo(customer);
        }

        [Benchmark]
        public void SerializeWithMessagePack()
        {
            var customer = Customer.CreateTestData(this.PayloadSize);
            #if NET462 || NETSTANDARD2_0
            var serializer = new DefaultCachedItemSerializer();
#else
            var serializer = new DefaultCachedItemSerializer(new MemoryPackSerializerOptions());
#endif
            var bytes = serializer.Serialize(customer);
            var deserializedCustomer = serializer.Deserialize<Customer>(bytes);
            deserializedCustomer.Should().BeEquivalentTo(customer);
        }
    }
}