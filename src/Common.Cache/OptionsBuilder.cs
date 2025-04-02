﻿// -----------------------------------------------------------------------
// <copyright file="OptionsBuilder.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Reflection;
    using System.Text.Json;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class OptionsBuilder
    {
        /// <summary>
        /// Adds the application settings to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="isFunctionApp">Indicates whether the application is a function app (default is false).</param>
        /// <param name="args">Command line arguments</param>
        /// <returns>The IConfiguration instance that was added to the service collection.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the appsettings.json file cannot be found.</exception>
        public static IConfiguration AddConfiguration(this IServiceCollection services, bool isFunctionApp = false, string[]? args = null)
        {
            var baseDirectory = Directory.GetCurrentDirectory();
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            if (isFunctionApp)
            {
                var webJobHome = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
                var home = Environment.GetEnvironmentVariable("HOME") == null
                    ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    : $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
                baseDirectory = (webJobHome ?? home) ?? baseDirectory;
                env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? env;
            }

            Console.WriteLine($"using base folder: {baseDirectory}");
            Console.WriteLine($"using environment: {env}");
            services.AddSingleton<IHostEnvironment>(new HostEnvironment(env));

            Console.WriteLine("registering app settings...");
            var appSettingFile = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            if (!File.Exists(appSettingFile))
            {
                throw new InvalidOperationException($"unable to find '{appSettingFile}'");
            }

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(baseDirectory)
                .AddJsonFile("appsettings.json", false, false);
            var appSettingOverrideFile = Path.Combine(baseDirectory, $"appsettings.{env}.json");
            if (File.Exists(appSettingOverrideFile))
            {
                Console.WriteLine($"found app setting override file: {appSettingOverrideFile}");
                configBuilder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, false);
            }

            configBuilder.AddEnvironmentVariables();

            if (args?.Length > 0)
            {
                configBuilder.AddCommandLine(args);
            }

            var config = configBuilder.Build();
            services.AddSingleton<IConfiguration>(config);

            return config;
        }

        /// <summary>
        /// Get configured settings without validation.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetConfiguredValue<T>(this IConfiguration configuration, string? sectionName = null)
            where T : class, new()
        {
            var configSection = GetConfigSection<T>(configuration, sectionName);
            var settings = new T();
            configSection.Bind(settings);
            return settings;
        }

        /// <summary>
        /// Gets strong-typed settings from the configuration without injecting IOptions&lt;T&gt;.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <param name="fallbackFill"></param>
        /// <typeparam name="T">Type of setting</typeparam>
        /// <returns><see cref="IServiceCollection"/></returns>
        /// <exception cref="InvalidOperationException">Throws when section name is invalid.</exception>
        public static T GetConfiguredSettings<T>(this IConfiguration configuration, string? sectionName = null, Action<T, T>? fallbackFill = null)
            where T : class, new()
        {
            var configSection = GetConfigSection<T>(configuration, sectionName);
            var settings = new T();
            configSection.Bind(settings);

            if (fallbackFill != null)
            {
                var fallbackSettings = new T();
                fallbackFill(fallbackSettings, settings);
            }

            var validationResult = ValidateSettings(settings);
            if (validationResult.Failed)
            {
                throw new InvalidOperationException($"Configuration setting {sectionName} failed for {typeof(T).Name}, {validationResult.FailureMessage}");
            }

            return settings;
        }

        private static IConfigurationSection GetConfigSection<T>(IConfiguration configuration, string? sectionName = null)
        {
            sectionName ??= typeof(T).Name;
            var sectionNameCamelCase = JsonNamingPolicy.CamelCase.ConvertName(sectionName);
            var configSection = configuration.GetSection(sectionName);
            if (!configSection.Exists())
            {
                configSection = configuration.GetSection(sectionNameCamelCase);
            }

            return configSection;
        }

        private static ValidateOptionsResult ValidateSettings<T>(T options) where T : class, new()
        {
            var validationResults = new List<ValidationResult>();
            if (Validator.TryValidateObject(options,
                    new ValidationContext(options, serviceProvider: null, items: null),
                    validationResults,
                    validateAllProperties: true))
            {
                return ValidateOptionsResult.Success;
            }

            var errors = new List<string>();
            foreach (var r in validationResults)
            {
                errors.Add($"DataAnnotation validation failed for members: '{string.Join(",", r.MemberNames)}' with the error: '{r.ErrorMessage}'.");
            }

            return ValidateOptionsResult.Fail(errors);
        }
    }
}