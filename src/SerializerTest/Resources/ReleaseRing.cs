//-------------------------------------------------------------------------------
// <copyright file="ReleaseRing.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum ReleaseRing
    {
        [EnumMember]
        Undefined = -1,
        [EnumMember]
        Canary = 0,
        [EnumMember]
        EarlyProduction = 1,
        [EnumMember]
        Production = 2
    }
}