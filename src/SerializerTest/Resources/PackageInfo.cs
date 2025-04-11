//------------------------------------------------------------------
// <copyright file="PackageInfo.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class models the package info.
    /// </summary>
    [DataContract]
    [Serializable]
    public class PackageInfo : IEquatable<PackageInfo>
    {
        /// <summary>
        /// Gets the Type of the package.
        /// </summary>
        [DataMember]
        public string Type { get; }

        /// <summary>
        /// Gets the Version of the package.
        /// </summary>
        [DataMember]
        public string Version { get; }

        /// <summary>
        /// Gets the MinVersionRequired of the package.
        /// </summary>
        [DataMember]
        public string MinVersionRequired { get; }

        /// <summary>
        /// Gets the publisher of the OEM/SBE package.
        /// </summary>
        [DataMember]
        public string Publisher { get; }

        /// <summary>
        /// Gets the family of the OEM/SBE package.
        /// </summary>
        [DataMember]
        public string Family { get; }

        /// <summary>
        /// Creates a new instance of the PackageInfo
        /// </summary>
        /// <param name="packageType">The type of the package</param>
        /// <param name="packageVersion">The version of the package, which can possibly be wildcard.</param>
        /// <param name="minVersionRequired">The minVersionRequired of the package</param>
        /// <param name="publisher">The publisher of the OEM/SBE package</param>
        /// <param name="family">The family of the OEM/SBE package</param>
        public PackageInfo(string packageType, string packageVersion, string minVersionRequired = "", string publisher = "", string family = "")
        {
            this.Type = packageType;
            this.Version = packageVersion;
            this.MinVersionRequired = minVersionRequired ?? packageVersion.Replace('*','0');
            this.Publisher = publisher;
            this.Family = family;
        }

        /// <inheritdoc/>
        public bool Equals(PackageInfo other)
        {
            return other != null &&
                string.Equals(this.Type, other.Type, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.Version, other.Version, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.MinVersionRequired, other.MinVersionRequired, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.Publisher, other.Publisher, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.Family, other.Family, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override bool Equals(Object other)
        {
            return this.Equals((PackageInfo)other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 753867696;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Version);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.MinVersionRequired);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Publisher);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Family);
            return hashCode;
        }

        public override string ToString()
        {
            var packageInfoString = $"{this.Type} {this.Version}";

            if (!string.IsNullOrEmpty(this.MinVersionRequired)
                && !string.Equals(this.Version.Replace('*', '0'), this.MinVersionRequired, StringComparison.InvariantCultureIgnoreCase))
            {
                packageInfoString += $" (MinVersionRequired: {this.MinVersionRequired})";
            }

            if (!string.IsNullOrEmpty(this.Publisher))
            {
                packageInfoString += $" Publisher: {this.Publisher}";
            }

            if (!string.IsNullOrEmpty(this.Family))
            {
                packageInfoString += $" Family: {this.Family}";
            }

            return packageInfoString;
        }
    }
}