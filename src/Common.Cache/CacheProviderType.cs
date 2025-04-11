// -----------------------------------------------------------------------
// <copyright file="CacheProviderType.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum CacheProviderType
    {
        [EnumMember]
        Null,
        [EnumMember]
        Memory,
        [EnumMember]
        Csv,
        [EnumMember]
        WindowsRegistry,
        [EnumMember]
        Hybrid
    }
}