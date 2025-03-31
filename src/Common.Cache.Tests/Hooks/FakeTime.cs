// -----------------------------------------------------------------------
// <copyright file="FakeTime.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Hooks
{
    using System;
    using Microsoft.Extensions.Internal;

    internal sealed class FakeTime : TimeProvider, ISystemClock
    {
        public void Reset() => this.UtcNow = DateTimeOffset.UtcNow;

        public DateTimeOffset UtcNow { get; private set; } = DateTimeOffset.UtcNow;

        public override DateTimeOffset GetUtcNow() => this.UtcNow;

        public void Add(TimeSpan delta) => this.UtcNow += delta;
    }
}