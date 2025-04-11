//------------------------------------------------------------------
// <copyright file="AvailabilityType.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Type of the update availability.
    /// </summary>
    [DataContract]
    public enum AvailabilityType
    {
        /// <summary>
        /// Update is available on local storage.
        /// </summary>
        [EnumMember]
        Local = 0,

        /// <summary>
        /// Update is available on line.
        /// </summary>
        [EnumMember]
        Online = 1,

        /// <summary>
        /// Update is notified by the third party.
        /// </summary>
        [EnumMember]
        Notify = 2
    }
}