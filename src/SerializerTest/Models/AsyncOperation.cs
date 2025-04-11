//------------------------------------------------------------------
// <copyright file="AsyncOperation.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This class represents the parent-child relationship for nested resources.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AsyncOperation
    {
        public Guid OperationId { get; set; }
    }

    /// <summary>
    /// This class represents the parent-child relationship for nested resources.
    /// </summary>
    /// <typeparam name="T">The type of arguments being passed to the operation.</typeparam>
    [ExcludeFromCodeCoverage]
    public class AsyncOperation<T> : AsyncOperation
    {
        public T OperationArguments { get; set; }
    }
}
