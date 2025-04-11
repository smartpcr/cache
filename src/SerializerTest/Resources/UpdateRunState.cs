//------------------------------------------------------------------
// <copyright file="UpdateRunState.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System.Runtime.Serialization;

    /// <summary>
    /// State of update run.
    /// </summary>
    [DataContract]
    public enum UpdateRunState
    {
        /// <summary>
        /// Update run state is unknown.
        /// </summary>
        [EnumMember]
        Unknown = 0,

        /// <summary>
        /// Update run is successful.
        /// </summary>
        [EnumMember]
        Succeeded = 1,

        /// <summary>
        /// Update run is in progress.
        /// </summary>
        [EnumMember]
        InProgress = 2,

        /// <summary>
        /// Update run failed.
        /// </summary>
        [EnumMember]
        Failed = 3,

        /// <summary>
        /// Health check succeeded
        /// </summary>
        [EnumMember]
        HealthCheckSucceeded = 4
    }
}