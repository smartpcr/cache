//------------------------------------------------------------------
// <copyright file="ArmResourceJsonConverter.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is used to deserialize ArmResources properly. In particular, it calls the constructor with an Id and then populates the object.
    /// </summary>
    public class ArmResourceJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ArmResource) ||
                  (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ArmResource<>));
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            var jObject = JObject.Load(reader);

            // call the ArmResource<T>(string id) constructor
            var idProperty = jObject.Property("id") ?? jObject.Property("Id");
            var obj = Activator.CreateInstance(
                type: objectType,
                bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                args: new object[] { idProperty?.Value.ToString() },
                culture: null);
            JsonConvert.PopulateObject(jObject.ToString(), obj);

            return obj;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Unnecessary because CanWrite is false. The type will skip the converter.
        }
    }
}
