// -----------------------------------------------------------------------
// <copyright file="WindowsRegistrySettings.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Win32;

    public class WindowsRegistryCacheSettings
    {
        [RegistryPathValidation]
        public string RegistryPath { get; set; }

        public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(15);
    }

    public class RegistryPathValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult("The registry path cannot be null or empty.");
            }

            if (value is string registryPath)
            {
                using var baseKey = Registry.LocalMachine.OpenSubKey(registryPath, writable: false);
                if (baseKey == null)
                {
                    return new ValidationResult($"The registry path specified in '{registryPath}' does not exist.");
                }
            }

            return ValidationResult.Success!;
        }
    }
}