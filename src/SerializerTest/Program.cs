namespace Microsoft.AzureStack.Services.Fabric.Common.Resource;

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using global::Common.Cache;
using global::Common.Cache.Serialization;
using global::Common.Models.Core;
using global::Common.Models.Resources;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Lib;

public class Program
{
    public static readonly Uri OtelEndpoint = new Uri("http://localhost:4317");
    private readonly IServiceProvider serviceProvider;

    public Program()
    {
        var services = new ServiceCollection();

        // setup otel
        var otelConfig = OtelSettings.GetOtelConfigSettings(true, OtelEndpoint.ToString(), LogLevel.Debug);
        var diagConfig = new DiagnosticsConfig(otelConfig, "CacheTestConsole");
        services.AddSingleton(diagConfig);

        // setup cache
        var clock = new SystemClock();
        var memoryCache = new MemoryCache(new MemoryCacheOptions { Clock = clock });
        services.AddSingleton<IMemoryCache>(memoryCache);

        var cacheFolder = Path.Combine(Directory.GetCurrentDirectory(), "caches");
        if (!Directory.Exists(cacheFolder))
        {
            Directory.CreateDirectory(cacheFolder);
        }
        var fileCache = new CsvFileCache(new CacheSettings()
        {
            CacheFolder = cacheFolder,
            TimeToLive = TimeSpan.FromMinutes(5)
        }, clock, DiagnosticsConfig.Instance);
        services.AddSingleton<IDistributedCache>(fileCache);

        services.AddSingleton<IHybridCacheSerializerFactory>(new SerializerFactory(SerializerType.Binary));
        services.AddHybridCache();

        // setup aggregator
        IResourceAggregator<Update> aggregator = new UpdateResourceAggregator();
        services.AddSingleton(aggregator);

        this.serviceProvider = services.BuildServiceProvider();
    }

    public static async Task Main(string[] args)
    {
        var program = new Program();
        program.UseBinarySerializer();
        // program.UseDataContractSerializer();

        await program.TestHybridCache();
    }

    private void UseDataContractSerializer()
    {
        var update = new Update("solution10.2508.1", null);
        var serializer = new DataContractSerializer(typeof(Update));
        string xml;

        // Serialize the person instance to XML.
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = XmlDictionaryWriter.CreateTextWriter(memoryStream))
            {
                serializer.WriteObject(writer, update);
            }

            xml = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        Console.WriteLine("Serialized XML:");
        Console.WriteLine(xml);
        Console.WriteLine();

        // Deserialize the XML back into a Person object.
        using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml)))
        {
            using (var reader = XmlDictionaryReader.CreateTextReader(memoryStream, new XmlDictionaryReaderQuotas()))
            {
                Update deserializedUpdate = (Update)serializer.ReadObject(reader);
                Console.WriteLine("Deserialized Update:");
                Console.WriteLine($"RpId: {deserializedUpdate.RpId.IdString}");
                Console.WriteLine($"Name: {deserializedUpdate.Name}");
            }
        }
    }

    private void UseBinarySerializer()
    {
        var update = new Update("solution10.2508.1", null);
        var stream = new MemoryStream();
        var formatter = new BinaryFormatter();
        formatter.Serialize(stream, update);

        stream.Seek(0, SeekOrigin.Begin);

        // Deserialize the object.
        var deserializedUpdate = (Update)formatter.Deserialize(stream);
        Console.WriteLine("Deserialized Update:");
        Console.WriteLine($"RpId: {deserializedUpdate.RpId.IdString}");
        Console.WriteLine($"Name: {deserializedUpdate.Name}");
    }

    private async Task TestHybridCache()
    {
        var hybridCache = this.serviceProvider.GetRequiredService<HybridCache>();
        var update = new Update("solution10.2508.1", null);
        var cacheKey = update.GetType().Name + update.RpId.IdString;
        await hybridCache.SetAsync(cacheKey, update);

        var cachedUpdate = await hybridCache.GetOrCreateAsync<Update>(
            cacheKey,
            _ => new ValueTask<Update>((Update)null),
            new HybridCacheEntryOptions()
            {
                Flags = HybridCacheEntryFlags.DisableDistributedCacheWrite | HybridCacheEntryFlags.DisableLocalCacheWrite
            });
        cachedUpdate.Should().NotBeNull();
        cachedUpdate.Should().BeOfType<Update>();
        cachedUpdate.Should().BeEquivalentTo(update);
    }

    private async Task TestResourceResolver(CancellationToken cancel)
    {
        var resolver = new ResourceResolver<Update>(this.serviceProvider);
        var updates = await resolver.GetResourceList(new ResolverContext(), cancel);
        updates.Should().NotBeNullOrEmpty();

        var rpId = RpIdFactory.Build("solution10.2508.1");
        var update = await resolver.GetResource(rpId, new ResolverContext(), cancel);
        update.Should().NotBeNull();
    }
}