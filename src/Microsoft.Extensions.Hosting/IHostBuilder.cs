// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// A builder for <see cref="IHost"/>.
    /// </summary>
    public interface IHostBuilder
    {
        /// <summary>
        /// Builds an <see cref="IHost"/> which hosts an application.
        /// </summary>
        IHost Build();

        /// <summary>
        /// Specify the <see cref="ILoggerFactory"/> to be used by the host.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to be used.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        IHostBuilder UseLoggerFactory(ILoggerFactory loggerFactory);

        /// <summary>
        /// Specify the delegate that is used to configure the services of the application.
        /// </summary>
        /// <param name="configureServices">The delegate that configures the <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        IHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);

        /// <summary>
        /// Adds a delegate for configuring the provided <see cref="ILoggerFactory"/>. This may be called multiple times.
        /// </summary>
        /// <param name="configureLogging">The delegate that configures the <see cref="ILoggerFactory"/>.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        IHostBuilder ConfigureLogging(Action<ILoggerFactory> configureLogging);

        /// <summary>
        /// Add or replace a setting in the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to add or replace.</param>
        /// <param name="value">The value of the setting to add or replace.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        IHostBuilder UseSetting(string key, string value);

        /// <summary>
        /// Get the setting value from the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to look up.</param>
        /// <returns>The value the setting currently contains.</returns>
        string GetSetting(string key);
    }
}
