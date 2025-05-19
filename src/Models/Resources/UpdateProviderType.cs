//------------------------------------------------------------------------------------------------
// <copyright file="UpdateProviderType.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------

namespace Common.Models.Resources
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Types of update providers
    /// </summary>
    [DataContract]
    public enum UpdateProviderType
    {
        /// <summary>
        /// Local file share update provider used to stage updates
        /// </summary>
        [EnumMember]
        LocalStorageProvider,

        /// <summary>
        /// Blob storage update provider used to discover side loaded updates
        /// </summary>
        [EnumMember]
        BlobStorageProvider,

        /// <summary>
        /// Oem update provider used to discover oem updates
        /// </summary>
        [EnumMember]
        OemUpdateProvider,

        /// <summary>
        /// Microsoft update provider used to discover microsoft updates
        /// </summary>
        [EnumMember]
        MicrosoftUpdateProvider,

        /// <summary>
        /// Component update provider used to discover component updates
        /// </summary>
        [EnumMember]
        ComponentUpdateProvider,

        /// <summary>
        /// Update discovery done outside the context of the update service.
        /// </summary>
        [EnumMember]
        StandaloneDiscovery
    }
}
