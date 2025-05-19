// -----------------------------------------------------------------------
// <copyright file="UpdateResourceAggregator.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Common.Models.Core;
    using global::Common.Models.Resources;

    public class UpdateResourceAggregator : IResourceAggregator<Update>
    {
        public async Task<Update> GetResult(RpId name, ResolverContext resolverContext = null, CancellationToken canel = default)
        {
            await Task.Delay(100, canel).ConfigureAwait(false);

            return new Update(name);
        }

        public async Task<IEnumerable<Update>> GetAllResults(ResolverContext resolverContext = null, CancellationToken canel = default)
        {
            var results = new List<Update>();
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(100, canel).ConfigureAwait(false);
                var rpId = RpIdFactory.Build($"solution10.250{i}.1.31");
                results.Add(new Update(rpId));
            }

            return results;
        }
    }
}