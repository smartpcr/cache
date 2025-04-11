//------------------------------------------------------------------
// <copyright file="RpId.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Models
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// This class uniquely identifies a resource within a resource provider. It implements
    /// <see cref="IEquatable{RpId}"/> which means <see cref="RpId"/> object can be compared
    /// using the <see cref="Equals(RpId)"/> method : it will not lead to a reference comparison
    /// but to a custom logic to determine if the RpIds are equal. It also means RpIds can be used
    /// as dictionary keys : the hash code wont be based on the reference of the object.
    /// </summary>
    public class RpId : PathBasedId<RpId>, IEquatable<RpId>
    {
        private string armName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RpId"/> class.
        /// </summary>
        /// <remarks>
        /// It cannot be made private because of generic constraint in the base <see cref="PathBasedId{T}"/> class,
        /// But it should only be used by the <see cref="PathBasedId{T}"/> class.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RpId()
        {
        }

        /// <summary>
        /// Gets the ARM resource name. This is in the format: parent_name/name.
        /// </summary>
        public string ArmName => this.armName ?? (this.armName = this.BuildNamePath());

        /// <summary>
        /// Verifies if the current object is equal to the <paramref name="other"/> <see cref="RpId"/>.
        /// </summary>
        /// <param name="other">The <see cref="RpId"/> the current object is to be compared to.</param>
        /// <returns>True if the current object is equal to the <paramref name="other"/>, else false.</returns>
        public bool Equals(RpId other)
        {
            return other != null && this.IdString.Equals(other.IdString, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var resourceIdContextObject = obj as RpId;
            if (resourceIdContextObject == null)
            {
                return false;
            }

            return this.Equals(resourceIdContextObject);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.IdString.ToLowerInvariant().GetHashCode();
        }
    }
}
