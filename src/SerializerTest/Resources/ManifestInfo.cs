//------------------------------------------------------------------
// <copyright file="ManifestInfo.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Linq;

    /// <summary>
    /// This class models the manifest info.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ManifestInfo : IEquatable<ManifestInfo>
    {
        /// <summary>
        /// Gets the Type of the update manifest.
        /// </summary>
        [DataMember]
        public ManifestType? ManifestType { get; }

        /// <summary>
        /// Gets the ReleaseType of the update manifest.
        /// </summary>
        [DataMember]
        public ReleaseType? ReleaseType { get; }

        /// <summary>
        /// Gets the CreationDate of the update maniest.
        /// </summary>
        [DataMember]
        public DateTime CreationDate { get; }

        /// <summary>
        /// Gets the CreatedBy of the update manifest.
        /// </summary>
        [DataMember]
        public CreatedByType? CreatedBy { get; }

        /// <summary>
        /// Creates a new instance of the ManifestInfo
        /// </summary>
        /// <param name="creationDate">The creation date of the update manifest.</param>
        /// <param name="createdBy">The created by of the update manifest.</param>
        /// <param name="manifestType">The type of the update manifest.</param>
        /// <param name="releaseType">The release type of the update manifest.</param>
        public ManifestInfo(DateTime creationDate, string createdBy = null, string manifestType = null, string releaseType = null)
        {
            this.CreationDate = creationDate;
            this.CreatedBy = Enum.TryParse(createdBy, out CreatedByType createdByVar) ? createdByVar : (CreatedByType?)null;
            this.ManifestType = Enum.TryParse(manifestType, out ManifestType manifestTypeVar) ? manifestTypeVar : (ManifestType?)null;
            this.ReleaseType = Enum.TryParse(releaseType, out ReleaseType releaseTypeVar) ? releaseTypeVar : (ReleaseType?)null;
        }

        /// <summary>
        /// Gets the manifest info metadata XML.
        /// </summary>
        public XElement GetMetadata()
        {
            var manifestInfo = new XElement("ManifestInfo");

            if (this.CreatedBy != null)
            {
                manifestInfo.SetAttributeValue("CreatedBy", this.CreatedBy);
            }

            if (this.ManifestType != null)
            {
                manifestInfo.SetAttributeValue("ManifestType", this.ManifestType);
            }

            if (this.ReleaseType != null)
            {
                manifestInfo.SetAttributeValue("ReleaseType", this.ReleaseType);
            }

            manifestInfo.SetAttributeValue("CreationDate", this.CreationDate.ToUniversalTime());

            return manifestInfo;
        }

        /// <inheritdoc/>
        public bool Equals(ManifestInfo other)
        {
            return other != null &&
                object.Equals(this.CreatedBy, other.CreatedBy) &&
                object.Equals(this.ManifestType, other.ManifestType) &&
                object.Equals(this.ReleaseType, other.ReleaseType);
        }

        /// <inheritdoc/>
        public override bool Equals(Object other)
        {
            return this.Equals((ManifestInfo)other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -495726192;
            hashCode = hashCode * -1521134295 + this.ManifestType.GetHashCode();
            hashCode = hashCode * -1521134295 + this.ReleaseType.GetHashCode();
            hashCode = hashCode * -1521134295 + this.CreatedBy.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// The type of the creation of the update manifest.
    /// </summary>
    [DataContract]
    public enum CreatedByType
    {
        [EnumMember]
        OEMBuild,
        [EnumMember]
        VaaS,
        [EnumMember]
        Manual
    }

    /// <summary>
    /// The type of the update manifest.
    /// </summary>
    [DataContract]
    public enum ManifestType
    {
        [EnumMember]
        OEM,
        [EnumMember]
        SBE,
        [EnumMember]
        Component,
        [EnumMember]
        Solution
    }

    /// <summary>
    /// The release type of the update manifest.
    /// </summary>
    [DataContract]
    public enum ReleaseType
    {
        [EnumMember]
        Draft,
        [EnumMember]
        ReleaseCandidate,
        [EnumMember]
        OEMRelease,
        [EnumMember]
        MicrosoftPublished
    }
}
