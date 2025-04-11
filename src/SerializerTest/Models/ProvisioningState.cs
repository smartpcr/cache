//-----------------------------------------------------------------------
// <copyright file="ProvisioningState.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Models
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Provisioning state for an operation in progress, or a resource being put.
    /// </summary>
    [DataContract]
    public enum ProvisioningState
    {
        /// <summary>
        /// Resource operation succeeded.
        /// </summary>
        [EnumMember]
        Succeeded = 0,

        /// <summary>
        /// Resource operation running.
        /// </summary>
        [EnumMember]
        Running,

        /// <summary>
        /// Resource operation failed.
        /// </summary>
        [EnumMember]
        Failed,

        /// <summary>
        /// Resource operation canceled.
        /// </summary>
        [EnumMember]
        Canceled,

        /// <summary>
        /// Resource accepted for creation.
        /// </summary>
        [EnumMember]
        Accepted,

        /// <summary>
        /// Resource under creation.
        /// </summary>
        [EnumMember]
        Creating,

        /// <summary>
        /// Resource under upgrade.
        /// </summary>
        [EnumMember]
        Upgrading,

        /// <summary>
        /// Resource being deleted.
        /// </summary>
        [EnumMember]
        Deleting
    }
}
