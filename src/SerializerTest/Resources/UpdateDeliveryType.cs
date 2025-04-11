//------------------------------------------------------------------
// <copyright file="UpdateDeliveryType.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Type of Update delivery mechanism.
    /// </summary>
    [DataContract]
    public enum DeliveryType
    {
        [EnumMember]
        Unknown,

        /// <summary>
        /// SBE update is notify only, customer needs to manually download.
        /// </summary>
        [EnumMember]
        Notify = 1,

        /// <summary>
        /// Update can be downloaded.
        /// </summary>
        [EnumMember]
        Distribute = 2,

        /// <summary>
        /// SBE update can be downloaded using download connector implemented by the OEM.
        /// </summary>
        [EnumMember]
        DownloadConnector = 3
    }
}
