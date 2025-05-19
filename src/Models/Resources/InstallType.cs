//-------------------------------------------------------------------------------
// <copyright file="InstallType.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace Common.Models.Resources
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
