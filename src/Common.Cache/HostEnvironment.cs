// -----------------------------------------------------------------------
// <copyright file="HostEnvironment.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    public interface IHostEnvironment
    {
        string EnvName { get; set; }
        bool IsDevelopment { get; }
    }

    public class HostEnvironment : IHostEnvironment
    {
        public string EnvName { get; set; }
        public bool IsDevelopment => this.EnvName == "Development";

        public HostEnvironment(string envName)
        {
            this.EnvName = envName;
        }
    }
}