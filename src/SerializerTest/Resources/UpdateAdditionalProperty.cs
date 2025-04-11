//------------------------------------------------------------------
// <copyright file="UpdateAdditionalProperty.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Resources
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public class UpdateAdditionalProperty
    {
        /// <summary>
        /// Gets or sets the key
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value
        {
            get;
            set;
        }
    }
}
