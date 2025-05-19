//------------------------------------------------------------------
// <copyright file="ArmIdFactory.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Common.Models.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::Common.Models.Core;

    /// <summary>
    /// This class contains methods to build <see cref="ArmId"/>.
    /// </summary>
    public static class ArmIdFactory
    {
        private const string ResourceIdNotProperlyFormed = "The resource id ({0}) is not properly formed.";

        /// <summary>
        /// The content of the provider type segment content
        /// </summary>
        public const string ProviderTypeSegmentContent = "providers";

        private const string SubscriptionPreSegment = "subscriptions";
        private const string ResourceGroupPreSegment = "resourceGroups";
        private const string ProviderPreSegment = "providers";

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
        public static ArmId Build(string[] resourceTypes, string[] resourceNames, int length = -1)
        {
            var armId = PathBasedIdFactory<ArmId>.Build(resourceTypes, resourceNames, length);
            ArmIdFactory.Initialize(armId);
            return armId;
        }

        /// <summary>
        /// Build a resource id based on resources types and resource names.
        /// </summary>
        /// <param name="subscriptionId">The subscription id to be used in the arm id.</param>
        /// <param name="resourceGroup">The resource group to be used in the arm id.</param>
        /// <param name="provider">The provider to be used in the arm id.</param>
        /// <param name="rpId">The <see cref="RpId"/> to be used in the last part of the arm id.</param>
        /// <returns>A properly formatted id.</returns>
        public static ArmId Build(string subscriptionId, string resourceGroup, string provider, RpId rpId)
        {
            var armId = ArmIdFactory.Build($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/{provider}{rpId.IdString}");
            return armId;
        }

        /// <summary>
        /// Builds a <see cref="ArmId"/> from a properly formated string : type1/value1/type2/value2 ...
        /// </summary>
        /// <param name="idString">The properly formatted string representing the id.</param>
        /// <returns>The equivalent <see cref="ArmId"/> object.</returns>
        public static ArmId Build(string idString)
        {
            var armId = PathBasedIdFactory<ArmId>.Build(idString);
            ArmIdFactory.Initialize(armId);
            return armId;
        }

        /// <summary>
        /// Builds a <see cref="ArmId"/> from a type, name, and parent id.
        /// </summary>
        /// <param name="type">The type of the resource to be appended.</param>
        /// <param name="name">The name of the resource to be appended.</param>
        /// <param name="parentRpId">The parent id.</param>
        /// <returns>A properly formatted id made of the previous <paramref name="parentRpId"/> and the <paramref name="name"/> and <paramref name="type"/>.</returns>
        public static ArmId Build(string type, string name, ArmId parentRpId = null)
        {
            var armId = PathBasedIdFactory<ArmId>.Build(type, name, parentRpId);
            ArmIdFactory.Initialize(armId);
            return armId;
        }

        /// <summary>
        /// Builds an ArmId from a URI.
        /// </summary>
        /// <param name="requestUri">The uri to build the arm id from.</param>
        /// <returns>The arm id.</returns>
        public static ArmId Build(Uri requestUri)
        {
            var localPath = requestUri.LocalPath;
            var url = localPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var values = url.Where((x, i) => i % 2 == 1).ToArray();
            var types = url.Where((x, i) => i % 2 == 0).ToArray();

            // we take value.Length for the length argument for GetAll calls : GetAll calls will not have a value behind the latest type.
            // In this specific case, we want to return the id of the parent.
            var armId = ArmIdFactory.Build(types.Take(values.Length).ToArray(), values);
            return armId;
        }

        /// <summary>
        /// Initialize ARM specific fields from a <see cref="ArmId"/>.
        /// </summary>
        /// <param name="resourceId">The resource ID to be parsed.</param>
        /// <exception cref="ArgumentException">Throws an argument exception if the <paramref name="resourceId"/> parameter is not properly formed.</exception>
        private static void Initialize(ArmId resourceId)
        {
            ArgumentValidator.NotNull(resourceId, nameof(resourceId));

            var resourceTypes = new List<string>();
            string subscriptionId = null, resourceGroupName = null, providerNamespace = null;

            var beyondProvider = true;

            var t = resourceId;
            while (t != null)
            {
                if (t.Type.Equals(ArmIdFactory.SubscriptionPreSegment, StringComparison.InvariantCultureIgnoreCase))
                {
                    subscriptionId = t.Name;
                }
                else if (t.Type.Equals(ArmIdFactory.ResourceGroupPreSegment, StringComparison.InvariantCultureIgnoreCase))
                {
                    resourceGroupName = t.Name;
                }
                else if (t.Type.Equals(ArmIdFactory.ProviderPreSegment, StringComparison.InvariantCultureIgnoreCase))
                {
                    providerNamespace = t.Name;
                    beyondProvider = false;
                }
                else if (beyondProvider)
                {
                    resourceTypes.Add(t.Type);
                }

                t = t.Parent;
            }

            if (providerNamespace == null)
            {
                throw new ArgumentException(string.Format(ArmIdFactory.ResourceIdNotProperlyFormed, resourceId), nameof(resourceId));
            }

            resourceTypes.Reverse();

            ArmIdFactory.InitializeRecursiveHelper(resourceId, subscriptionId, resourceGroupName, providerNamespace, resourceTypes);
        }

        /// <summary>
        /// Recursive helpers used by <see cref="Initialize"/> method to initialize fields on parent objects.
        /// </summary>
        /// <param name="resourceId">The resource ID to be parsed.</param>
        /// <param name="subscriptionId">The subscriptionId is used only for recursion: use default value.</param>
        /// <param name="resourceGroupName">The resourceGroupName is used only for recursion: use default value.</param>
        /// <param name="providerNamespace">The providerNamespace is used only for recursion: use default value.</param>
        /// <param name="resourceTypes">The resourceTypes is used only for recursion: use default value.</param>
        private static void InitializeRecursiveHelper(
            ArmId resourceId,
            string subscriptionId,
            string resourceGroupName,
            string providerNamespace,
            List<string> resourceTypes)
        {
            resourceId.ProviderNameSpace = resourceId.Type.Equals(ArmIdFactory.ResourceGroupPreSegment, StringComparison.InvariantCultureIgnoreCase) ? null : providerNamespace;
            resourceId.SubscriptionId = subscriptionId;
            resourceId.ResourceGroupName = resourceId.Type.Equals(ArmIdFactory.SubscriptionPreSegment, StringComparison.InvariantCultureIgnoreCase) ? null : resourceGroupName;
            resourceId.ResourceType = resourceId.Type.Equals(ArmIdFactory.ResourceGroupPreSegment, StringComparison.InvariantCultureIgnoreCase) ? null : $"{resourceId.ProviderNameSpace}/{string.Join("/", resourceTypes)}";

            if (resourceId.Parent != null)
            {
                ArmIdFactory.InitializeRecursiveHelper(resourceId.Parent, subscriptionId, resourceGroupName, providerNamespace, resourceTypes.Take(Math.Max(0, resourceTypes.Count - 1)).ToList());
            }
        }
    }
}
