//------------------------------------------------------------------
// <copyright file="RebootRequirement.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Common.Models.Resources
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The Reboot Requirement of the update.
    /// </summary>
    [DataContract]
    public enum RebootRequirement
    {
        /// <summary>
        /// It's unknown if reboot is required. (e.g. nobody tested it yet)
        /// </summary>
        [EnumMember]
        Unknown,

        /// <summary>
        /// Reboot is required.
        /// </summary>
        [EnumMember]
        Yes,

        /// <summary>
        /// Reboot is not required.
        /// </summary>
        [EnumMember]
        No
    }
}
