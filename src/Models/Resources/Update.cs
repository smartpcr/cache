//------------------------------------------------------------------
// <copyright file="Update.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Common.Models.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml.Linq;
    using Common.Models.Core;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// This class models an Update resource.
    /// </summary>
    [DataContract]
    [Serializable]
    public class Update : BaseResourceProperties, IEquatable<Update>
    {
        /// <summary>
        /// The name of this resource type.
        /// </summary>
        public const string ResourceTypeName = "updates";

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Update"/> class.
        /// </summary>
        /// <param name="resourceId">The id of this resource.</param>
        [ExcludeFromCodeCoverage]
        public Update(RpId resourceId)
            : base(resourceId)
        {
            this.UpdateStateProperties = new UpdateStateProperties();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Update"/> class.
        /// </summary>
        /// <param name="name">The name of this resource.</param>
        /// <param name="parentId">The id of the parent of this resource.</param>
        public Update(string name, RpId parentId)
            : base(Update.ResourceTypeName, name, parentId)
        {
            this.UpdateStateProperties = new UpdateStateProperties();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Update"/> class.
        /// </summary>
        /// <param name="version">The update version.</param>
        /// <param name="parentId">The id of the parent of this resource.</param>
        public Update(Version version, RpId parentId)
            : this(version.ToString(), parentId)
        {
            this.Version = version;
            this.UpdateStateProperties = new UpdateStateProperties();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the date the update was installed.
        /// </summary>
        [DataMember]
        public DateTime? InstalledDate { get; set; }

        /// <summary>
        /// Gets or sets the description of the update.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the update state.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public UpdateState State { get; set; }

        /// <summary>
        /// Gets or sets the KB article link of the update, ReleaseLink will be recommended in since this version.
        /// </summary>
        [DataMember]
        public string KbLink { get; set; }

        /// <summary>
        /// Gets or sets the release link of the update.
        /// </summary>
        public string ReleaseLink { get; set; }

        /// <summary>
        /// Gets or sets the additional properties.
        /// </summary>
        [DataMember]
        public List<UpdateAdditionalProperty> AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the minimum required version to apply the update.
        /// </summary>
        [JsonConverter(typeof(VersionConverter))]
        [DataMember]
        public Version MinVersionRequired { get; set; }

        /// <summary>
        /// Gets or sets the minimum required OEM version to apply the update.
        /// </summary>
        [JsonConverter(typeof(VersionConverter))]
        [DataMember]
        public Version MinSbeVersionRequired { get; set; }

        /// <summary>
        /// Gets or sets the directory path of the update package.
        /// </summary>
        [DataMember]
        public string PackagePath { get; set; }

        /// <summary>
        /// Gets or sets the package provider of the update package.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public UpdateProviderType PackageProviderType { get; set; }

        /// <summary>
        /// Gets or sets the size of the update package.
        /// </summary>
        [DataMember]
        public uint PackageSizeInMb { get; set; }

        /// <summary>
        /// Gets or sets the size of the update package.
        /// </summary>
        [DataMember]
        public uint PrivatePackageSizeInMb { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public override string Type => Update.ResourceTypeName;

        /// <summary>
        /// Gets or sets the name of the update.
        /// </summary>
        [IgnoreDataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the package type of the update.
        /// </summary>
        [DataMember]
        public string PackageType { get; set; }

        /// <summary>
        /// Gets or sets the version of the update.
        /// </summary>
        [JsonConverter(typeof(VersionConverter))]
        [DataMember]
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the OEM version of the update.
        /// </summary>
        [JsonConverter(typeof(VersionConverter))]
        [DataMember]
        public Version SbeVersion { get; set; }

        /// <summary>
        /// Gets or sets the update publisher.
        /// </summary>
        [DataMember]
        public string Publisher { get; set; }

        /// <summary>
        /// Gets or sets the OEM family.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public string OemFamily { get; set; }

        /// <summary>
        /// Gets or sets the supported models of the OEM update.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public IEnumerable<SupportedModel> SupportedModels { get; set; } = new List<SupportedModel>();

        /// <summary>
        /// Gets or sets the notify message.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public string OemNotifyMsg { get; set; }

        /// <summary>
        /// Gets or sets the license URI associated with the SBE download connector.
        /// </summary>
        [DataMember]
        public string SbeLicenseUri { get; set; }

        /// <summary>
        /// Gets or sets the copyright message associated with the SBE download connector.
        /// </summary>
        [DataMember]
        public string SbeCopyright { get; set; }

        /// <summary>
        /// Gets or sets the delivery type.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public DeliveryType DeliveryType { get; set; }

        /// <summary>
        /// Gets or sets the RebootRequirement type.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public RebootRequirement RebootRequired { get; set; }

        /// <summary>
        /// Gets or sets the install type.
        /// </summary>
        [DataMember]
        public InstallType InstallType { get; set; }

        /// <summary>
        /// Gets or sets the prerequisite packages of the update
        /// </summary>
        [DataMember]
        public List<UpdatePrerequisite> Prerequisites { get; set; }

        /// <summary>
        /// Gets or sets the update state properties
        /// </summary>
        [DataMember]
        public UpdateStateProperties UpdateStateProperties { get; set; }

        /// <summary>
        /// Gets or sets the update AvailabilityType
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public AvailabilityType AvailabilityType { get; set; }

        // <summary>
        /// Gets or sets the aggregated state of update prechecks.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public HealthState HealthState { get; set; }

        /// <summary>
        /// Gets or sets the last time an update precheck was completed.
        /// </summary>
        [DataMember]
        public DateTime? HealthCheckDate { get; set; }

        /// <summary>
        /// Gets or sets the required Azure Stack version string which may contain wildcards.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public string VersionRequired { get; set; }

        /// <summary>
        /// Gets or sets the required OEM version string, which may contain wildcards.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public string OemVersionRequired { get; set; }

        /// <summary>
        /// Gets or sets the manifest info of the update.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public ManifestInfo ManifestInfo { get; set; }

        /// <summary>
        /// Gets or sets the OS info of the update.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public OSInfo OSInfo { get; set; }

        /// <summary>
        /// Gets or sets the ValidatedConfigurations of the update.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public IEnumerable<ValidatedConfiguration> ValidatedConfigurations { get; set; } = new List<ValidatedConfiguration>();

        /// <summary>
        /// Gets or sets the UpdateImpact of the update.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public IEnumerable<UpdateImpact> UpdateImpact { get; set; } = new List<UpdateImpact>();

        /// <summary>
        /// The set of child updates of the solution update.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public List<Update> ChildUpdates { get; set; }

        /// <summary>
        /// Gets or sets the component versions for a Solution Bundle update, and an empty collection otherwise.
        /// </summary>
        /// <remarks>Although ChildUpdates is a superset of this information, this property is added to facilitate
        /// more straight forward mapping from the core model to the client model in the controller.</remarks>
        [DataMember]
        public List<PackageVersionInfo> ComponentVersions => this.MapChildUpdatesToComponentVersions();

        /// <summary>
        /// Gets or sets the IsRecalled value for the update.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public bool IsRecalled { get; set; }

        /// <summary>
        /// Gets or sets the manifest containing Platform Consistency Enforcement information.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public string PlatformConsistencyEnforcementManifest { get; set; }

        /// <summary>
        /// Gets or sets the has attempted preparation flag
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public bool HasAttemptedPreparation { get; set; }

        /// <summary>
        /// Gets or sets the zip file
        /// </summary>
        [DataMember]
        public UpdateZipFile ZipFile { get; set; }

        /// <summary>
        /// Gets or sets the metadata file
        /// </summary>
        [DataMember]
        public UpdateMetadataFile MetadataFile { get; set; }

        /// <summary>
        /// Gets or sets the release ring
        /// </summary>
        [DataMember]
        public ReleaseRing ReleaseRing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scan and download of this update should be deferred to post-preparation.
        /// </summary>
        [DataMember]
        public bool DeferScanAndDownload { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string ToString() => this.GetMetadata().ToString();

        public override int GetHashCode()
        {
            return this.RpId.IdString.GetHashCode();
        }

        /// <summary>
        /// Gets the metadata representing this instance.
        /// </summary>
        /// <returns>The metadata.</returns>
        public XDocument GetMetadata()
        {
            var updateInfo = new XElement("UpdateInfo");
            updateInfo.Add(new XElement("UpdateName", this.DisplayName));
            updateInfo.Add(new XElement("PackageSizeInMb", this.PrivatePackageSizeInMb));
            updateInfo.Add(new XElement("MinVersionRequired", this.MinVersionRequired));
            updateInfo.Add(new XElement("MinOemVersionRequired", this.MinSbeVersionRequired));
            updateInfo.Add(new XElement("InstallType", this.InstallType));

            if (this.Version != null)
            {
                updateInfo.Add(new XElement("Version", this.Version));
            }

            if (this.SbeVersion != null)
            {
                updateInfo.Add(new XElement("OEMVersion", this.SbeVersion));
            }

            if (!string.IsNullOrWhiteSpace(this.Publisher))
            {
                updateInfo.Add(new XElement("Publisher", this.Publisher));
            }

            if (!string.IsNullOrWhiteSpace(this.Description))
            {
                updateInfo.Add(new XElement("Description", this.Description));
            }

            if (!string.IsNullOrWhiteSpace(this.KbLink))
            {
                updateInfo.Add(new XElement("KBLink", this.KbLink));
            }

            if (!string.IsNullOrWhiteSpace(this.OemFamily))
            {
                updateInfo.Add(new XElement("OEMFamily", this.OemFamily));
            }

            if (!string.IsNullOrWhiteSpace(this.PackageType))
            {
                updateInfo.Add(new XElement("PackageType", this.PackageType));
            }

            if (this.SupportedModels != null)
            {
                var supportedModels = new XElement("SupportedModels");
                foreach (var model in this.SupportedModels)
                {
                    supportedModels.Add(model.ToXElement());
                }

                updateInfo.Add(supportedModels);
            }

            if (this.ZipFile != null)
            {
                updateInfo.Add(new XElement("PackageHash", this.ZipFile.Hash));
            }

            var updatePackageManifest = new XElement("UpdatePackageManifest");

            if (!string.IsNullOrWhiteSpace(this.OemNotifyMsg) && Enum.IsDefined(typeof(DeliveryType), this.DeliveryType))
            {
                var fileInfo = new XElement("File");
                fileInfo.SetAttributeValue("NotifyMsg", this.OemNotifyMsg);
                fileInfo.SetAttributeValue("Type", this.DeliveryType.ToString());
                fileInfo.SetAttributeValue("Name", this.DisplayName);
                fileInfo.SetAttributeValue("Uri", string.Empty);
                updatePackageManifest.Add(fileInfo);
            }

            if (this.MetadataFile != null)
            {
                var metadataFileInfo = new XElement("MetadataFile");
                metadataFileInfo.SetAttributeValue("Name", this.MetadataFile.Name);
                metadataFileInfo.SetAttributeValue("Uri", this.MetadataFile.Uri);
                updatePackageManifest.Add(metadataFileInfo);
            }

            if (Enum.IsDefined(typeof(RebootRequirement), this.RebootRequired))
            {
                updateInfo.Add(new XElement("RebootRequired", this.RebootRequired.ToString()));
            }

            if (this.ValidatedConfigurations.Count() > 0)
            {
                var validatedConfigurations = this.GetValidatedConfigurationsMetadata();
                var validatedConfigurationsElement = new XElement("ValidatedConfigurations");
                validatedConfigurationsElement.Add(validatedConfigurations);
                updatePackageManifest.AddFirst(validatedConfigurationsElement);
            }

            if (this.ChildUpdates != null && this.ChildUpdates.Count() > 0)
            {
                var updateComposition = this.GetChildUpdatesMetadata();
                updatePackageManifest.Add(updateComposition);
            }

            if (this.OSInfo != null)
            {
                var osInfo = new XElement("OS");

                osInfo.SetAttributeValue("Branch", this.OSInfo.Branch);
                osInfo.SetAttributeValue("Product", this.OSInfo.Product);
                osInfo.SetAttributeValue("SKU", this.OSInfo.SKU);

                if (this.OSInfo.Hotpatch != null)
                {
                    var hotPatchInfo = new XElement("Hotpatch");
                    hotPatchInfo.SetAttributeValue("Version", this.OSInfo.Hotpatch.Version);
                    hotPatchInfo.SetAttributeValue("BaselineVersion", this.OSInfo.Hotpatch.BaselineVersion);
                    osInfo.Add(hotPatchInfo);
                }

                if (this.OSInfo.Coldpatch != null)
                {
                    var coldPatchInfo = new XElement("Coldpatch");
                    coldPatchInfo.SetAttributeValue("Version", this.OSInfo.Coldpatch.Version);
                    osInfo.Add(coldPatchInfo);
                }

                updateInfo.Add(osInfo);
            }

            if (this.ManifestInfo != null)
            {
                updatePackageManifest.AddFirst(this.ManifestInfo.GetMetadata());
            }

            updatePackageManifest.Add(updateInfo);

            var metadata = new XDocument { Declaration = new XDeclaration("1.0", Encoding.UTF8.WebName, "yes") };
            metadata.Add(updatePackageManifest);
            return metadata;
        }

        /// <summary>
        /// Get the validated configurations from the update metadata.
        /// </summary>
        /// <returns>The required packages element.</returns>
        public IEnumerable<XElement> GetValidatedConfigurationsMetadata()
        {
            var validatedConfigurations = new List<XElement>();
            foreach (var validatedConfiguration in this.ValidatedConfigurations)
            {
                validatedConfigurations.Add(this.GetRequiredPackagesMetadata(validatedConfiguration));
            }

            return validatedConfigurations;
        }

        /// <summary>
        /// Get the RequiredPackages metadata from the validatedConfiguration object.
        /// </summary>
        /// <returns>The required packages element.</returns>
        public XElement GetRequiredPackagesMetadata(ValidatedConfiguration validatedConfiguration)
        {
            var requiredPackages = new XElement("RequiredPackages");
            foreach (var requiredPackage in validatedConfiguration.RequiredPackages.ToList())
            {
                var pkgInfo = new XElement("Package");
                pkgInfo.SetAttributeValue("Type", requiredPackage.Type);
                if (!String.IsNullOrEmpty(requiredPackage.Version))
                {
                    pkgInfo.SetAttributeValue("Version", requiredPackage.Version);
                }

                if (!String.IsNullOrEmpty(requiredPackage.MinVersionRequired))
                {
                    pkgInfo.SetAttributeValue("MinVersionRequired", requiredPackage.MinVersionRequired);
                }

                if (!String.IsNullOrEmpty(requiredPackage.Publisher))
                {
                    pkgInfo.SetAttributeValue("Publisher", requiredPackage.Publisher);
                }

                if (!String.IsNullOrEmpty(requiredPackage.Family))
                {
                    pkgInfo.SetAttributeValue("Family", requiredPackage.Family);
                }

                requiredPackages.Add(pkgInfo);
            }

            return requiredPackages;
        }

        /// <summary>
        /// Get the solution update metadata of the update.
        /// </summary>
        /// <param name="update">The update</param>
        /// <returns>The solution update metadata</returns>
        private static XElement GetSolutionUpdateMetadata(Update update)
        {
            var pkgInfo = new XElement("Update");
            pkgInfo.SetAttributeValue("Type", update.PackageType);

            var version = !String.IsNullOrEmpty(update.Version?.ToString()) ? update.Version.ToString() : update.SbeVersion?.ToString();
            if (!String.IsNullOrEmpty(version))
            {
                pkgInfo.SetAttributeValue("Version", version);
            }

            if (!String.IsNullOrEmpty(update.Publisher))
            {
                pkgInfo.SetAttributeValue("Publisher", update.Publisher);
            }

            if (!String.IsNullOrEmpty(update.OemFamily))
            {
                pkgInfo.SetAttributeValue("Family", update.OemFamily);
            }

            return pkgInfo;
        }

        /// <summary>
        /// Get the child updates referenced in the solution update.
        /// </summary>
        /// <returns>The SolutionMetadata element contains the full metadata (UpdateInfo) of the child updates.</returns>
        public XElement GetChildUpdatesMetadata()
        {
            var solutionMetadata = new XElement("SolutionMetadata");

            foreach (var update in this.ChildUpdates)
            {
                var childUpdateEntry = Update.GetSolutionUpdateMetadata(update);
                var updateMetadata = update.GetMetadata();
                childUpdateEntry.Add(updateMetadata.Descendants("UpdateInfo"));
                if (updateMetadata.Descendants("ManifestInfo").FirstOrDefault() != null)
                {
                    childUpdateEntry.Add(updateMetadata.Descendants("ManifestInfo"));
                }

                if (updateMetadata.Descendants("MetadataFile").FirstOrDefault() != null)
                {
                    childUpdateEntry.Add(updateMetadata.Descendants("MetadataFile"));
                }

                solutionMetadata.Add(childUpdateEntry);
            }

            return solutionMetadata;
        }

        /// <inheritdoc/>
        public bool Equals(Update other)
        {
            return
                other != null &&
                object.Equals(this.Version, other.Version) &&
                object.Equals(this.MinVersionRequired, other.MinVersionRequired) &&
                object.Equals(this.MinSbeVersionRequired, other.MinSbeVersionRequired) &&
                object.Equals(this.PackageProviderType, other.PackageProviderType) &&
                string.Equals(this.DisplayName, other.DisplayName, StringComparison.Ordinal) &&
                string.Equals(this.Description, other.Description, StringComparison.Ordinal) &&
                string.Equals(this.PackageType, other.PackageType, StringComparison.Ordinal) &&
                string.Equals(this.KbLink, other.KbLink, StringComparison.Ordinal) &&
                string.Equals(this.Publisher, other.Publisher, StringComparison.Ordinal) &&
                string.Equals(this.OemFamily, other.OemFamily, StringComparison.Ordinal) &&
                (this.ChildUpdates == null || this.ChildUpdates.SequenceEqual(other.ChildUpdates)) &&
                (this.ValidatedConfigurations == null || this.ValidatedConfigurations.SequenceEqual(other.ValidatedConfigurations)) &&
                object.Equals(this.OSInfo, other.OSInfo) &&
                object.Equals(this.ManifestInfo, other.ManifestInfo) &&
                object.Equals(this.InstallType, other.InstallType) &&
                object.Equals(this.RebootRequired, other.RebootRequired) &&
                object.Equals(this.SbeVersion, other.SbeVersion) &&
                object.Equals(this.PrivatePackageSizeInMb, other.PrivatePackageSizeInMb);
        }

        /// <summary>
        /// Maps the more detailed list of ChildUpdates to the simplified ComponentVersions list.
        /// </summary>
        /// <returns></returns>
        private List<PackageVersionInfo> MapChildUpdatesToComponentVersions()
        {
            return this.ChildUpdates == null ? null : this.ChildUpdates
                .Select(x => new PackageVersionInfo()
                {
                    PackageType = x.PackageType,
                    Version = x.Version?.ToString() ?? x.SbeVersion.ToString(),
                    LastUpdated = null
                })
                .ToList();
        }

        #endregion
    }
}