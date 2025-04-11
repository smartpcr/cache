//-------------------------------------------------------------------------------
// <copyright file="ValidatedConfiguration.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class models the validated configration.
    /// </summary>
    [DataContract]
    public class ValidatedConfiguration : IEquatable<ValidatedConfiguration>
    {
        /// <summary>
        /// The set of required packages of the validated configuration.
        /// </summary>
        [DataMember]
        public IEnumerable<PackageInfo> RequiredPackages;

        /// <summary>
        /// Creates a new instance of the validated configuration.
        /// </summary>
        /// <param name="requiredPackages">The required packages.</param>
        public ValidatedConfiguration(IEnumerable<PackageInfo> requiredPackages)
        {
            this.RequiredPackages = requiredPackages;
        }

        /// <inheritdoc/>
        public bool Equals(ValidatedConfiguration other)
        {
            return other != null &&
                this.RequiredPackages.SequenceEqual(other.RequiredPackages);
        }

        /// <inheritdoc/>
        public override bool Equals(Object other)
        {
            return other is ValidatedConfiguration validatedConfiguration && this.Equals(validatedConfiguration);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return 591740417 + EqualityComparer<IEnumerable<PackageInfo>>.Default.GetHashCode(this.RequiredPackages);
        }
    }
}
