//------------------------------------------------------------------
// <copyright file="PathBasedId.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Common.Models.Core
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class describes path based ids such as type1/value1/type2/value2
    /// </summary>
    /// <typeparam name="T">The type of the parent id (used to have a strong type recursion).</typeparam>
    [DataContract]
    [Serializable]
    public abstract class PathBasedId<T>
        where T : PathBasedId<T>
    {
        /// <summary>
        ///  Gets the id string of the resource.
        /// </summary>
        [DataMember]
        public string IdString { get; internal set; }

        /// <summary>
        ///  Gets the name of the resource.
        /// </summary>
        [DataMember]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the parent resource.
        /// </summary>
        [DataMember]
        public T Parent { get; internal set; }

        /// <summary>
        /// Gets the type of the resource.
        /// </summary>
        [DataMember]
        public string Type { get; internal set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.IdString;
        }

        /// <summary>
        /// Initialize a path based id with type, name, and parents.
        /// </summary>
        /// <param name="type">The type of the resource to be identified.</param>
        /// <param name="name">The name of the resource to be identified.</param>
        /// <param name="parent">The i of the parent of the resource to be identified.</param>
        internal virtual void Initialize(string type, string name, T parent = null)
        {
            this.Type = type;
            this.Name = name;
            this.Parent = parent;
            var array = this.Parent == null ? new[] { this.Type, this.Name } : new[] { this.Parent.IdString.Substring(1), this.Type, this.Name };
            this.IdString = "/" + string.Join("/", array);
        }

        /// <summary>
        /// Builds the name path from the id string, walking up the chain and only stopping at the specified stopAtType. The id string consists of type/name pairs, the
        /// name path is a reduction of the id string to only the names.
        /// </summary>
        /// <example>
        /// If the id string is: subscriptions/sub0/resourceGroups/rg0/providers/Microsoft.Test.provider/type1/value1/type2/value2,
        ///
        /// Then the ARM resource name with no stopAtType would be: sub0/rg0/Microsoft.Test.provider/value1/value2.
        ///
        /// With the stopAtType as: providers, then the full name would be: value1/value2.
        /// </example>
        /// <param name="stopAtType">The type string to stop walking at, or <c>null</c> to build with all of the names in the id string.</param>
        /// <returns>The name path.</returns>
        protected string BuildNamePath(string stopAtType = null)
        {
            if (this.Parent != null && (stopAtType == null || this.Parent.Type != stopAtType))
            {
                string parentFullName = this.Parent.BuildNamePath(stopAtType);
                return $"{parentFullName}/{this.Name}";
            }

            return this.Name;
        }
    }
}
