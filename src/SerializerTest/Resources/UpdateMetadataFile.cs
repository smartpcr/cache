//-------------------------------------------------------------------------------
// <copyright file="UpdateMetadataFile.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;

    /// <summary>
    /// Represents the update's metadata file.
    /// </summary>
    [Serializable]
    public class UpdateMetadataFile
    {
        /// <summary>
        /// Gets or sets the name of the update metadata file.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the URI of the update metadata file.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMetadataFile"/> class.
        /// </summary>
        /// <param name="name">The name of the update metadata file.</param>
        /// <param name="uri">The URI of the update metadata file.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="name"/> or <paramref name="uri"/> is null.
        /// </exception>
        public UpdateMetadataFile(string name, string uri)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }
    }
}
