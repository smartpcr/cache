//----------------------------------------
// <copyright file="BaseResourceProperties.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//----------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Models
{
    using System.Runtime.Serialization;
    using Contract;
    using Newtonsoft.Json;

    /// <summary>
    /// Define the base behavior for class implementing the <see cref="IResourceProperties"/> interface
    /// </summary>
    [DataContract]
    public abstract class BaseResourceProperties : IResourceProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResourceProperties"/> class.
        /// </summary>
        /// <param name="resourceId">The id of this resource.</param>
        protected BaseResourceProperties(RpId resourceId)
        {
            ArgumentValidator.NotNull(resourceId, nameof(resourceId));
            this.RpId = resourceId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResourceProperties"/> class based on its type,name and its parent.
        /// </summary>
        /// <param name="type">The type of this resource.</param>
        /// <param name="name">The name of this resource.</param>
        /// <param name="parentId">The id of the parent of this resource.</param>
        protected BaseResourceProperties(string type, string name, RpId parentId)
            : this(RpIdFactory.Build(type, name, parentId))
        {
        }

        /// <inheritdoc />
        [JsonIgnore]
        [IgnoreDataMember]
        public RpId RpId { get; }

        /// <inheritdoc />
        [JsonIgnore]
        [IgnoreDataMember]
        public string Name => this.RpId.Name;

        /// <inheritdoc/>
        [JsonIgnore]
        [IgnoreDataMember]
        public abstract string Type { get; }

        public virtual string GetCacheKey()
        {
            // id string starts with '/' already
            return $"{this.GetType().Name}/{this.RpId.IdString.TrimStart('/')}";
        }
    }
}