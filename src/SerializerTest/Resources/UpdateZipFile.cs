//-------------------------------------------------------------------------------
// <copyright file="UpdateZipFile.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the update's zip file used in the update process.
    /// </summary>
    [DataContract]
    public class UpdateZipFile
    {
        /// <summary>
        /// Gets or sets the name of the zip file.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URI of the zip file.
        /// If the zip file exists and the URI is empty, it means we expect this file download to be handled by someone else.
        /// For example, an SBE Dell APEXCP update where the download is handled by the Dell APEX WAC extension and Download Connector.
        /// </summary>
        [DataMember]
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the hash of the zip file.
        /// </summary>
        [DataMember]
        public string Hash { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateZipFile"/> class.
        /// </summary>
        /// <param name="name">The name of the zip file.</param>
        /// <param name="uri">The URI of the zip file.</param>
        /// <param name="hash">The hash of the zip file.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="name"/>, <paramref name="uri"/>, or <paramref name="hash"/> is null.
        /// </exception>
        public UpdateZipFile(string name, string uri, string hash)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            this.Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }
    }
}
