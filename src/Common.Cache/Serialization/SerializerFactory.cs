// -----------------------------------------------------------------------
// <copyright file="SerializerFactory.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Serialization
{
    using Microsoft.Extensions.Caching.Hybrid;

    public class SerializerFactory : IHybridCacheSerializerFactory
    {
        private readonly SerializerType serializerType;

        public SerializerFactory(SerializerType serializerType)
        {
            this.serializerType = serializerType;
        }

        public bool TryCreateSerializer<T>(out IHybridCacheSerializer<T> serializer)
        {
            switch (this.serializerType)
            {
                case SerializerType.Binary:
                    serializer = new DefaultBinarySerializer<T>();
                    return true;
                case SerializerType.Xml:
                    serializer = new DataContractSerializer<T>();
                    return true;
                case SerializerType.TextJson:
                    serializer = new DefaultTextSerializer<T>();
                    return true;
                case SerializerType.NewtonsoftJson:
                    serializer = new DefaultJsonSerializer<T>();
                    return true;
                case SerializerType.MsgPack:
                    serializer = new MsgPackSerializer<T>();
                    return true;
                default:
                    serializer = null;
                    return false;
            }
        }
    }
}