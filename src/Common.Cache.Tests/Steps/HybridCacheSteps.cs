// -----------------------------------------------------------------------
// <copyright file="HybridCacheSteps.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Steps
{
    using Reqnroll;

    [Binding]
    public class HybridCacheSteps
    {
        private readonly ScenarioBlock context;
        private readonly IReqnrollOutputHelper outputHelper;

        public HybridCacheSteps(ScenarioBlock context, IReqnrollOutputHelper outputHelper)
        {
            this.context = context;
            this.outputHelper = outputHelper;
        }
    }
}