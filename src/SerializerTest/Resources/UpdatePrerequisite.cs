//------------------------------------------------------------------
// <copyright file="UpdatePrerequisite.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class models is the prerequisite package information for an update.
    /// </summary>
    [DataContract]
    public class UpdatePrerequisite : IEquatable<UpdatePrerequisite>
    {
        /// <summary>
        /// Gets or sets the update type
        /// </summary>
        [DataMember]
        public string UpdateType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the prerequisite update.
        /// </summary>
        [DataMember]
        public string PackageName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the prerequisite update.
        /// </summary>
        [DataMember]
        public string Version
        {
            get;
            set;
        }

        public override bool Equals(Object other)
        {
            return this.Equals((UpdatePrerequisite)other);
        }

        /// <inheritdoc/>
        public bool Equals(UpdatePrerequisite other)
        {
            return other != null &&
                String.Equals(this.UpdateType, other.UpdateType, StringComparison.OrdinalIgnoreCase) &&
                String.Equals(this.Version, other.Version) &&
                String.Equals(this.PackageName, other.PackageName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            int hashCode = 753867696;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.UpdateType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Version);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.PackageName);
            return hashCode;
        }
    }
}
