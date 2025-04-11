//-----------------------------------------------------------------------
// <copyright file="IResourcePropertiesMappingHandler.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Contract
{
    using System;
    using Models;

    /// <summary>
    /// Provides methods to map core model <see cref="IResourceProperties"/> objects to their client model
    /// representations, and vice a versa.
    /// </summary>
    /// <typeparam name="TCore">The core model type.</typeparam>
    public interface IResourcePropertiesMappingHandler<TCore>
        where TCore : IResourceProperties
    {
        /// <summary>
        /// Determines the client type to use for the specified version.
        /// </summary>
        /// <remarks>
        /// By default, the client type is the same as the core type, override this in your controller when using a different client type.
        /// </remarks>
        /// <param name="apiVersion">The version.</param>
        /// <returns>The type of properties to return to the client inside of an <see cref="ArmResource"/>.</returns>
        Type GetClientTypeForVersion(string apiVersion);

        /// <summary>
        /// Maps a core model object to a client model object. Usually we get the core model object by calling to our core components, like when satisfying a
        /// GET request, and then we need to transform it to its final client representation before returning it to the client.
        /// </summary>
        /// <param name="coreProperties">The core object to map.</param>
        /// <param name="apiVersion">the version information to use to pick the correct client model representation.</param>
        /// <returns>The mapped properties used by the client model.</returns>
        object MapCoreModelToClientModel(TCore coreProperties, string apiVersion);

        /// <summary>
        /// Maps a client model object to a core model object. Usually the client object is provided by the client in a PUT or POST call.
        /// </summary>
        /// <param name="clientProperties">The client object to map.</param>
        /// <param name="rpId">The resource provider id of the object to convert.</param>
        /// <param name="apiVersion">The apiVersion of the client call.</param>
        /// <returns>The mapped properties used by the core model.</returns>
        TCore MapClientModelToCoreModel(object clientProperties, RpId rpId, string apiVersion);

        /// <summary>
        /// Maps an <see cref="ArmResource"/> that we got from an external PUT or POST call to the appropriate client model.
        /// </summary>
        /// <param name="resource">The ARM resource.</param>
        /// <param name="apiVersion">
        /// The version information in the call, use this information to determine the correct client model to convert to.
        /// </param>
        /// <returns>
        /// The converted client model object, deserialized from the client provided properties. Any properties not expected
        /// are ignored, and any properties not supplied are set to their defaults.
        /// </returns>
        object ExtractClientModelFromArmResource(ArmResource resource, string apiVersion);
    }
}
