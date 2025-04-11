namespace Microsoft.AzureStack.Services.Fabric.Common.Resource;

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources;

public class Program
{
    public static void Main(string[] args)
    {
        UseBinarySerializer();
    }

    private static void UseDataContractSeriaizer()
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

    private static void UseBinarySerializer()
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
}