// -----------------------------------------------------------------------
// <copyright file="IResourceAggregator.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Common.Models.Contract;
    using global::Common.Models.Core;

    /// <summary>
    /// Used to get a single, more easily displayable object representing a fabric resource which contains data from multiple lower level objects.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    public interface IResourceAggregator<T> where T : class, IResourceProperties
    {
        /// <summary>
        /// Gets a single resource by name from an external component.
        /// </summary>
        /// <param name="name">Name of the object to aggregate.</param>
        /// <param name="resolverContext">Additional parameters to the <see cref="IResourceAggregator{T}"/>
        /// are passed using <see cref="ResolverContext" />.</param>
        /// <returns>Task containing the result.</returns>
        Task<T> GetResult(RpId name, ResolverContext resolverContext = null, CancellationToken canel = default);

        /// <summary>
        /// Gets a collection of resources from an external component.
        /// </summary>
        /// <param name="resolverContext">Additional parameters(like <see cref="ArmId" />) to the Aggregator
        /// are passed using <see cref="ResolverContext" /></param>
        /// <returns>Task containing all results.</returns>
        Task<IEnumerable<T>> GetAllResults(ResolverContext resolverContext = null, CancellationToken canel = default);
    }
}