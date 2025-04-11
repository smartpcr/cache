﻿//------------------------------------------------------------------
// <copyright file="UpdateImpact.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------


namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class models the UpdateImpact.
    /// </summary>
    [DataContract]
    public class UpdateImpact
    {
        [DataMember]
        public string InstalledVersion { get; }

        [DataMember]
        public RebootRequirement RebootRequired { get; }

        public UpdateImpact(string installedVersion, string rebootRequired)
        {
            this.InstalledVersion = installedVersion;
            this.RebootRequired = Enum.TryParse(rebootRequired, out RebootRequirement rebootRequiredVar) ? rebootRequiredVar : RebootRequirement.Unknown;
        }
    }
}
