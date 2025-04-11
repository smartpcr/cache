//----------------------------------------
// <copyright file="IResourceProperties.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//----------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Contract
{
    using Models;

    /// <summary>
    /// This interface defines the contract that resource properties must match to be used within an <see cref="ArmResource"/>
    /// </summary>
    public interface IResourceProperties
    {
        /// <summary>
        /// Gets the unique identifier of a resourceProperties object.
        /// This field is used to map resource properties that are being cached,
        /// with the corresponding ARM Resource that are to wrap the properties.
        /// </summary>
        /// <remarks>
        /// It has no setter as the IResourceProperties implementation should be able to provide a
        /// predictable unique identifier : in case of crash, the cache will be rebuild and the
        /// predictable identifier can be used to associate the resource with ARM envelopes stored in
        /// reliable storage.
        /// </remarks>
        RpId RpId { get; }

        /// <summary>
        /// Gets the name of the resource.
        /// </summary>
        /// <remarks>
        /// This field should NOT be serialized as it would duplicate the Name field of the <see cref="ArmResource"/>
        /// object that wraps this object.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the type of this resource as displayed in the ids and routes.
        /// </summary>
        /// <remarks>
        /// This field should NOT be serialized as it would duplicate the Name field of the <see cref="ArmResource"/>
        /// object that wraps this object.
        /// </remarks>
        string Type { get; }
    }
}
