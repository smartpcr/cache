//------------------------------------------------------------------
// <copyright file="HealthState.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System.Runtime.Serialization;

    /// <summary>
    /// State of an update precheck execution.
    /// </summary>
    [DataContract]
    public enum HealthState
    {
        /// <summary>
        /// Default value representing an unknown state. This state will be used when system health check has never been run.
        /// </summary>
        [EnumMember]
        Unknown = 0,

        /// <summary>
        /// All prechecks have succeeded.
        /// </summary>
        [EnumMember]
        Success = 1,

        /// <summary>
        /// At least one precheck of level Critical has failed.
        /// </summary>
        [EnumMember]
        Failure = 2,

        /// <summary>
        /// At least one precheck of level Warning has failed.
        /// </summary>
        [EnumMember]
        Warning = 3,

        /// <summary>
        /// There was a failure invoking the health check.
        /// </summary>
        [EnumMember]
        Error = 4,

        /// <summary>
        /// A precheck is currently in progress.
        /// </summary>
        [EnumMember]
        InProgress = 5
    }
}