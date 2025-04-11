//------------------------------------------------------------------
// <copyright file="OSInfo.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class models the OS info of the platform update.
    /// </summary>
    [DataContract]
    public class OSInfo : IEquatable<OSInfo>
    {
        /// <summary>
        /// The hotpatch info.
        /// </summary>
        [DataContract]
        public class HotpatchInfo
        {
            [DataMember]
            public Version Version;

            [DataMember]
            public Version BaselineVersion;

            public HotpatchInfo(string version, string baselineVersion)
            {
                this.Version = Version.Parse(version);
                this.BaselineVersion = Version.Parse(baselineVersion);
            }
        }

        /// <summary>
        /// The coldpatch info.
        /// </summary>
        [DataContract]
        public class ColdpatchInfo
        {
            [DataMember]
            public Version Version;

            public ColdpatchInfo(string version)
            {
                this.Version = Version.Parse(version);
            }
        }

        /// <summary>
        /// Gets the branch of the OS update.
        /// </summary>
        [DataMember]
        public string Branch { get; }

        /// <summary>
        /// Gets the Product of the OS update.
        /// </summary>
        [DataMember]
        public string Product { get; }

        /// <summary>
        /// Gets the SKU of the OS update.
        /// </summary>
        [DataMember]
        public uint SKU { get; }

        /// <summary>
        /// Gets the hotpatch info of the update.
        /// </summary>
        [DataMember]
        public HotpatchInfo Hotpatch { get; }

        /// <summary>
        /// Gets the coldpatch info of the update.
        /// </summary>
        [DataMember]
        public ColdpatchInfo Coldpatch { get; }

        /// <summary>
        /// Creates a new instance of the OSInfo
        /// </summary>
        /// <param name="version">The OS version of the update.</param>
        /// <param name="branch">The OS branch of the update.</param>
        public OSInfo(string branch, string product, uint sku, HotpatchInfo hotpatchInfo, ColdpatchInfo coldpatchInfo)
        {
            this.Branch = branch;
            this.Product = product;
            this.SKU = sku;
            this.Hotpatch = hotpatchInfo;
            this.Coldpatch = coldpatchInfo;
        }

        /// <inheritdoc/>
        public bool Equals(OSInfo other)
        {
            return other != null &&
                string.Equals(this.Branch, other.Branch, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.Product, other.Product, StringComparison.OrdinalIgnoreCase) &&
                object.Equals(this.SKU, other.SKU) &&
                Version.Equals(this.Coldpatch?.Version, other.Coldpatch?.Version) &&
                Version.Equals(this.Hotpatch?.Version, other.Hotpatch?.Version) &&
                Version.Equals(this.Hotpatch?.BaselineVersion, other.Hotpatch?.BaselineVersion);
        }

        /// <inheritdoc/>
        public override bool Equals(Object other)
        {
            return this.Equals((OSInfo)other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 720547488;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Branch);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Product);
            hashCode = hashCode * -1521134295 + this.SKU.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<HotpatchInfo>.Default.GetHashCode(this.Hotpatch);
            hashCode = hashCode * -1521134295 + EqualityComparer<ColdpatchInfo>.Default.GetHashCode(this.Coldpatch);
            return hashCode;
        }
    }
}
