//------------------------------------------------------------------
// <copyright file="PathBasedIdFactory.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Models
{
    using System;
    using System.Linq;

    /// <summary>
    /// This class creates <see cref="PathBasedId{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the parent id (used to have a strong type recursion).</typeparam>
    public static class PathBasedIdFactory<T> where T : PathBasedId<T>, new()
    {
        private const string TheResourceNamesArrayMustContainsTheSameNumberOfElementsAsTheResourceTypesArray = "The ResourceNames array must contain the same number of element as the ResourceTypes array to construct a ParentResourceObject.";
        private const string AtLeastATypeAndANameArerequiredToBuildAProperResourceId = "At least a type and a name are required to build a proper resource id.";
        /// <summary>
        /// Builds a <see cref="PathBasedId{T}"/> of type T.
        /// </summary>
        /// <param name="resourceTypes">The type of the ids.</param>
        /// <param name="resourceNames">The values of the ids.</param>
        /// <param name="length">The length of the array that must be taken in account.</param>
        /// <returns>A <see cref="PathBasedId{T}"/> of type T.</returns>
        public static T Build(string[] resourceTypes, string[] resourceNames, int length = -1)
        {
            ArgumentValidator.IsTrue(
                resourceNames.Length == resourceTypes.Length,
                nameof(resourceNames),
                PathBasedIdFactory<T>.TheResourceNamesArrayMustContainsTheSameNumberOfElementsAsTheResourceTypesArray);

            length = length == -1 ? resourceTypes.Length : length;

            ArgumentValidator.IsInRange(
                length,
                1,
                null,
                nameof(resourceTypes),
                PathBasedIdFactory<T>.AtLeastATypeAndANameArerequiredToBuildAProperResourceId);

            var type = resourceTypes[length - 1];
            var name = resourceNames[length - 1];
            var parent = length > 1 ? PathBasedIdFactory<T>.Build(resourceTypes, resourceNames, length - 1) : null;

            return PathBasedIdFactory<T>.Build(type, name, parent);
        }

        /// <summary>
        /// Builds a <see cref="PathBasedId{T}"/> of type T.
        /// </summary>
        /// <param name="type">The type of the resource to be identified.</param>
        /// <param name="name">The name of the resource to be identified.</param>
        /// <param name="parent">The id of the parent of the resource to be identified.</param>
        /// <returns>A <see cref="PathBasedId{T}"/> of type T.</returns>
        public static T Build(string type, string name, T parent)
        {
            var id = new T();
            id.Initialize(type, name, parent);

            return id;
        }

        /// <summary>
        /// Builds a <see cref="RpId"/> from a properly formated string : type1/value1/type2/value2 ...
        /// </summary>
        /// <param name="idString">The properly formatted string representing the id.</param>
        /// <returns>The equivalent <see cref="RpId"/> object.</returns>
        public static T Build(string idString)
        {
            var splitArray = idString.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var names = splitArray.Where((x, i) => i % 2 == 1).ToArray();
            var types = splitArray.Where((x, i) => i % 2 == 0).ToArray();

            return PathBasedIdFactory<T>.Build(types, names);
        }
    }
}
