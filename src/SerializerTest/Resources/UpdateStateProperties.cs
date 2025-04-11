//------------------------------------------------------------------
// <copyright file="UpdateStateProperties.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System.Runtime.Serialization;

    /// <summary>
    /// This class models is the state properties of an update.
    /// </summary>
    [DataContract]
    public class UpdateStateProperties
    {
        /// <summary>
        /// Gets or sets the notify message
        /// </summary>
        [DataMember]
        public string NotifyMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the progress percentage of a long term run state.
        /// </summary>
        [DataMember]
        public int ProgressPercentage
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.NotifyMessage))
            {
                return this.ProgressPercentage > 0 && this.ProgressPercentage < 100 ? $"{this.ProgressPercentage}% complete." : string.Empty;
            }

            // NotifyMessage is only applicable if the update is not available to download directly.
            return this.NotifyMessage;
        }
    }
}
