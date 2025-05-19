//------------------------------------------------------------------
// <copyright file="UpdateRun.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Common.Models.Resources
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using Common.Models.Core;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// This class models an UpdateRun resource.
    /// </summary>
    [DataContract]
    [Serializable]
    public class UpdateRun : BaseResourceProperties
    {
        /// <summary>
        /// The name of this resource type.
        /// </summary>
        public const string ResourceTypeName = "updateRuns";

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateRun"/> class.
        /// </summary>
        /// <param name="resourceId">The id of this resource.</param>
        [ExcludeFromCodeCoverage]
        public UpdateRun(RpId resourceId)
            : base(resourceId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateRun"/> class.
        /// </summary>
        /// <param name="name">The name of this resource.</param>
        /// <param name="parentId">The id of the parent of this resource.</param>
        public UpdateRun(string name, RpId parentId)
            : base(UpdateRun.ResourceTypeName, name, parentId)
        {
        }

        /// <summary>
        /// Gets or sets the progress of the update run.
        /// </summary>
        [DataMember]
        public Step Progress { get; set; }

        /// <summary>
        /// Gets or sets the time when the update run started.
        /// </summary>
        [DataMember]
        public DateTime? TimeStarted { get; set; }

        /// <summary>
        /// Gets or sets the completion time of the last completed step, if any.
        /// </summary>
        [DataMember]
        public DateTime? LastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets the duration of the update run.
        /// </summary>
        [JsonConverter(typeof(Iso8601TimeSpanConverter))]
        [DataMember]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the state of update run.
        /// </summary>
        /// [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public UpdateRunState State { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the on complete action succeeded.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public bool OnCompleteActionSuccess { get; set; }

        /// <summary>
        /// Gets or sets the additional data in this property of an update run, for internal use.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public int PreparationDownloadPercentage { get; set; }

        /// <summary>
        /// Gets or sets whether the update run is for preparation.
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public bool IsPreparationRun { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public override string Type => UpdateRun.ResourceTypeName;
    }
}
