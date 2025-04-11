//-----------------------------------------------------------------------
// <copyright file="ResourceCoreModelAttribute.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Contract
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AzureStack.Services.Fabric.Common.Resource.Models;

    /// <summary>
    /// An attribute to apply to client model types to declare their corresponding core model type.
    /// </summary>
    /// <remarks>
    /// This attribute could be expanded in the future to include mapping information from the client model to the core model.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [ExcludeFromCodeCoverage]
    public sealed class ResourceCoreModelAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceCoreModelAttribute"/> class.
        /// </summary>
        /// <param name="coreModelType">The core model type.</param>
        public ResourceCoreModelAttribute(Type coreModelType)
        {
            ArgumentValidator.NotNull(coreModelType, nameof(coreModelType));
            ArgumentValidator.IsTrue(typeof(IResourceProperties).IsAssignableFrom(coreModelType), nameof(coreModelType), null);

            this.CoreModelType = coreModelType;
        }

        /// <summary>
        /// Gets the core model type.
        /// </summary>
        public Type CoreModelType { get; }
    }
}
