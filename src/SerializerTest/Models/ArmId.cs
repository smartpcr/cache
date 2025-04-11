//------------------------------------------------------------------
// <copyright file="ArmId.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Models
{
    using System.ComponentModel;

    /// <summary>
    /// This class represents the parent-child relationship for nested resources.
    /// </summary>
    public class ArmId : PathBasedId<ArmId>
    {
        private string armName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmId"/> class.
        /// </summary>
        /// <remarks>
        /// It cannot be made private because of generic constraint in the base <see cref="PathBasedIdFactory{T}"/> class,
        /// But it should only be used by the <see cref="ArmIdFactory"/> class.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ArmId()
        {
        }

        /// <summary>
        /// Gets or sets the id of the subscription the resource belongs to.
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource group the resource belongs to.
        /// </summary>
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the namespace of the provider that provides the resource.
        /// </summary>
        public string ProviderNameSpace { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource. Eg: parentType/Type
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets the ARM resource name. This is in the format: parent_name/name.
        /// </summary>
        public string ArmName => this.armName ?? (this.armName = this.BuildNamePath(stopAtType: "providers"));
    }
}
