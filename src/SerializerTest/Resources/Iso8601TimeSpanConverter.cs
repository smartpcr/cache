//-------------------------------------------------------------------------
// <copyright file="Iso8601TimeSpanConverter.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using Newtonsoft.Json;

    /// <summary>
    /// Converts <see cref="TimeSpan"/> to <see cref="string"/> in ISO format.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class Iso8601TimeSpanConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => serializer.Serialize(writer, XmlConvert.ToString((TimeSpan)value));

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => XmlConvert.ToTimeSpan(serializer.Deserialize<string>(reader));

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType) => typeof(TimeSpan).IsAssignableFrom(objectType);
    }
}