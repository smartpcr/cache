//------------------------------------------------------------------
// <copyright file="RpIdFactory.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Common.Models.Core
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// This class contains methods to build <see cref="RpId"/>.
    /// </summary>
    public static class RpIdFactory
    {
        /// <summary>
        /// Builds an <see cref="RpId"/> from a <see cref="ArmId"/>.
        /// </summary>
        /// <param name="armId">The ARM id to be transformed into a resource provider id.</param>
        /// <returns>The resulting resource provider id.</returns>
        public static RpId Build(ArmId armId)
        {
            if (armId == null)
            {
                return null;
            }

            var armIdString = armId.IdString;
            var providerSegmentIndex = armIdString.IndexOf(ArmIdFactory.ProviderTypeSegmentContent, StringComparison.InvariantCultureIgnoreCase);
            Debug.Assert(providerSegmentIndex != -1, "ARM id should contain a providers segment. Check that the id is properly formed.");

            var rpIdWithProvider = armIdString.Substring(providerSegmentIndex + ArmIdFactory.ProviderTypeSegmentContent.Length + 1);
            var firstSlashIndex = rpIdWithProvider.IndexOf('/');

            // if there is no slash beyond the provider, then it does not correspond to an RpId.
            // Eg: subscription/sub0/resourcegroups/rg0/providers/Microsoft.Fabric.Admin
            if (firstSlashIndex == -1)
            {
                return null;
            }

            var rpId = rpIdWithProvider.Substring(firstSlashIndex + 1);

            return RpIdFactory.Build(rpId);
        }

        /// <summary>
        /// Build a resource id based on resources types and resource names.
        /// </summary>
        /// <param name="resourceTypes">Contains the ancestry chain of types. eg:{grand parent type, parent type, current type}.</param>
        /// <param name="resourceNames">Contains the ancestry chain of names. eg:{grand parent name, parent name, current name}.</param>
        /// <param name="length">
        /// When specified only a subset of the type and names arrays, starting at index 0, and of length
        /// <paramref name="length"/> will be considered.
        /// </param>
        /// <returns>A properly formatted id.</returns>
        public static RpId Build(string[] resourceTypes, string[] resourceNames, int length = -1)
        {
            var rpId = PathBasedIdFactory<RpId>.Build(resourceTypes, resourceNames, length);

            if (resourceTypes.Last().Equals(ArmIdFactory.ProviderTypeSegmentContent, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return rpId;
        }

        /// <summary>
        /// Builds a <see cref="RpId"/> from a type, name, and parent id.
        /// </summary>
        /// <param name="type">The type of the resource to be appended.</param>
        /// <param name="name">The name of the resource to be appended.</param>
        /// <param name="parentRpId">The parent id.</param>
        /// <returns>A properly formatted id made of the previous <paramref name="parentRpId"/> and the <paramref name="name"/> and <paramref name="type"/>.</returns>
        public static RpId Build(string type, string name, RpId parentRpId = null)
        {
            return PathBasedIdFactory<RpId>.Build(type, name, parentRpId);
        }

        /// <summary>
        /// Builds a <see cref="RpId"/> from a properly formated string : type1/value1/type2/value2 ...
        /// </summary>
        /// <param name="idString">The properly formatted string representing the id.</param>
        /// <returns>The equivalent <see cref="RpId"/> object.</returns>
        public static RpId Build(string idString)
        {
            return PathBasedIdFactory<RpId>.Build(idString);
        }
    }
}
