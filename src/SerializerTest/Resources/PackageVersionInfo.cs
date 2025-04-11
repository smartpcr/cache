//------------------------------------------------------------------
// <copyright file="PackageVersionInfo.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class models is the package information for an Azure Stack Hub stamp.
    /// </summary>
    [DataContract]
    [Serializable]
    public class PackageVersionInfo
    {
        /// <summary>
        /// Gets or sets the update type
        /// </summary>
        [DataMember]
        public string PackageType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the package
        /// </summary>
        [DataMember]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last update time of the package
        /// </summary>
        [DataMember]
        public DateTime? LastUpdated
        {
            get;
            set;
        }

        /// <summary>
        /// Overrides the PackageVersionInfo type
        /// </summary>
        public override string ToString()
        {
            if (this.LastUpdated != null)
            {
                string lastUpdatedDate = this.LastUpdated.ToString();
                return $"{this.PackageType}: {this.Version} - last updated on: {lastUpdatedDate} ";
            }
            else
            {
                return $"{this.PackageType}: {this.Version}";
            }
        }
    }
}
