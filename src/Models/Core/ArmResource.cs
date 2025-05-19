//----------------------------------------
// <copyright file="ArmResource.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//----------------------------------------

namespace Common.Models.Core
{
    using System;
    using System.Collections.Generic;
    using Contract;
    using Newtonsoft.Json;

    /// <summary>
    /// This class describes an ARM resource.
    /// </summary>
    public class ArmResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArmResource"/> class.
        /// </summary>
        public ArmResource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmResource"/> class. This constructor is used by
        /// the JSON converter <see cref="ArmResourceJsonConverter"/>.
        /// </summary>
        /// <param name="id">The id of the ArmResource. This is currently ignored in the base class. We keep this constructor here so that it
        /// matches the constructor in <see cref="ArmResource{T}"/>.</param>
        internal ArmResource(string id)
        {
        }

        /// <summary>
        /// Gets or sets the ID of a resource : the entire url(except for the host) that allows one to access the resource :
        /// similar to /subscriptions/foo/resourceGroups/bar/providers/MS.SQL/databases/myDB
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the resource's name.
        /// </summary>
        [JsonIgnore]
        public string ResourceName
        {
            get
            {
                if (this.Name == null)
                {
                    return null;
                }

                // Clean out any extra ending '/' in the Name.
                string name = this.Name.TrimEnd('/');

                // Find the last slash.
                int slashIndex = name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);

                if (slashIndex == -1)
                {
                    return name;
                }

                string resourceName = name.Substring(slashIndex + 1);

                return resourceName;
            }
        }

        /// <summary>
        /// Gets or sets the name of the ARM resource. The ARM resource name consists of its full hierarchy,
        /// whereas the <see cref="ResourceName"/> is the name of just this resource.
        /// </summary>
        /// <example>If we have an InfraRoleInstance, its ResourceName will be something like VM01, and its Name will be: local/VM01, where local is its
        /// parent's name.</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of a resource
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Location of a resource
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the tags of a resource.
        /// Tags are a set of key value pairs attached to a resource. They are user defined
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the kind.
        /// This is an optional field to be used by the UI
        /// </summary>
        [JsonIgnore]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the ETag.
        /// This field is used in concurrent update scenario:
        ///  - every update start with a GET
        ///  - then the client POST the updated object
        ///  - when the POSt is received by the server, the server verifies that the ETAG matches the one of the stored resources:
        ///    - if it does the resource is updated and receives a new ETAG
        ///    - if it does not the resource is not updated and the server returns an error code (conflict)
        /// </summary>
        [JsonIgnore]
        public string Etag { get; set; }

        /// <summary>
        /// Gets or sets the custom properties of an ARM resource.
        /// </summary>
        [JsonProperty(propertyName: "Properties")]
        public object PropertiesObject { get; set; }
    }

    /// <summary>
    /// A specialization of the <see cref="ArmResource"/> class, where the type of the properties is known.
    /// </summary>
    /// <typeparam name="T">The properties type.</typeparam>
    public class ArmResource<T> : ArmResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArmResource{T}"/> class.
        /// </summary>
        public ArmResource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmResource{T}"/> class. This constructor is used by
        /// the JSON converter <see cref="ArmResourceJsonConverter"/>.
        /// </summary>
        /// <param name="id">The id of the ArmResource.</param>
        internal ArmResource(string id)
            : base(id)
        {
            // If T is a core model type, then call its constructor that takes a RPId (If an id was not passed to the constructor, then leave the Properties object as null).
            // Otherwise, this is a client model type and these all just have a default constructor.
            if (typeof(IResourceProperties).IsAssignableFrom(typeof(T)))
            {
                if (id != null)
                {
                    var armId = ArmIdFactory.Build(id);
                    var rpId = RpIdFactory.Build(armId);

                    this.Properties = (T)Activator.CreateInstance(typeof(T), rpId);
                }
            }
            else
            {
                this.Properties = (T)Activator.CreateInstance(typeof(T));
            }
        }

        /// <summary>
        /// Gets or sets the custom properties of an ARM resource.
        /// </summary>
        [JsonIgnore]
        public T Properties
        {
            get { return (T)this.PropertiesObject; }
            set { this.PropertiesObject = value; }
        }
    }
}