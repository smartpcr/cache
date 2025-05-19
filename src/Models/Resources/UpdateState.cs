//------------------------------------------------------------------
// <copyright file="UpdateState.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Common.Models.Resources
{
    using System.Runtime.Serialization;

    /// <summary>
    /// State of an update.
    /// </summary>
    [DataContract]
    public enum UpdateState
    {
        /// <summary>
        /// Update cannot be installed because it has some prerequisite.
        /// </summary>
        [EnumMember]
        HasPrerequisite = 0,

        /// <summary>
        /// Update cannot be installed because it is obsolete, it has no run.
        /// </summary>
        [EnumMember]
        Obsolete,

        /// <summary>
        /// Update is applicable.
        /// </summary>
        [EnumMember]
        Ready,

        /// <summary>
        /// Update is not applicable because another update is in progress.
        /// </summary>
        [EnumMember]
        NotApplicableBecauseAnotherUpdateIsInProgress,

        /// <summary>
        /// The update is being downloaded to the infra share or is being extracted.
        /// </summary>
        [EnumMember]
        Preparing,

        /// <summary>
        /// Update is being installed.
        /// </summary>
        [EnumMember]
        Installing,

        /// <summary>
        /// Update has already been installed successfully.
        /// </summary>
        [EnumMember]
        Installed,

        /// <summary>
        /// Download or extraction failed.
        /// </summary>
        [EnumMember]
        PreparationFailed,

        /// <summary>
        /// Update has not been installed successfully.
        /// </summary>
        [EnumMember]
        InstallationFailed,

        /// <summary>
        /// Update is invalid for the stamp.
        /// </summary>
        [EnumMember]
        Invalid,

        /// <summary>
        /// Update is recalled.
        /// </summary>
        [EnumMember]
        Recalled,

        /// <summary>
        /// Update is downloading.
        /// </summary>
        [EnumMember]
        Downloading,

        /// <summary>
        /// Update download failed.
        /// </summary>
        [EnumMember]
        DownloadFailed,

        /// <summary>
        /// Update is running health check.
        /// </summary>
        [EnumMember]
        HealthChecking,

        /// <summary>
        /// Update health check failed.
        /// </summary>
        [EnumMember]
        HealthCheckFailed,

        /// <summary>
        /// Update is ready to install.
        /// </summary>
        [EnumMember]
        ReadyToInstall,

        /// <summary>
        /// Additional update content is required before the update can be ready.
        /// </summary>
        [EnumMember]
        AdditionalContentRequired
    }

    public static class UpdateStateEx
    {
        public static bool IsTerminalState(this UpdateState state)
        {
            return state == UpdateState.Installed ||
                   state == UpdateState.InstallationFailed ||
                   state == UpdateState.Invalid ||
                   state == UpdateState.Recalled ||
                   state == UpdateState.Obsolete ||
                   state == UpdateState.HasPrerequisite ||
                   state == UpdateState.NotApplicableBecauseAnotherUpdateIsInProgress ||
                   state == UpdateState.AdditionalContentRequired ||
                   state == UpdateState.PreparationFailed ||
                   state == UpdateState.DownloadFailed ||
                   state == UpdateState.HealthCheckFailed;
        }

        public static bool IsInProgressState(this UpdateState state)
        {
            return state == UpdateState.Preparing ||
                   state == UpdateState.Installing ||
                   state == UpdateState.Downloading ||
                   state == UpdateState.HealthChecking ||
                   state == UpdateState.Ready ||
                   state == UpdateState.ReadyToInstall;
        }
    }
}