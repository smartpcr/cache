//-------------------------------------------------------------------------------
// <copyright file="InstallType.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System.Runtime.Serialization;

    // The Update InstallType
    [DataContract]
    public enum InstallType
    {
        [EnumMember]
        Update = 0,

        [EnumMember]
        Hotfix
    }
}
