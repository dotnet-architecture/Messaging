// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Use the given configuration settings on the host.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> containing settings to be used.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder UseConfiguration(this IHostBuilder hostBuilder, IConfiguration configuration)
        {
            foreach (var setting in configuration.AsEnumerable())
            {
                hostBuilder.UseSetting(setting.Key, setting.Value);
            }

            return hostBuilder;
        }

        /// <summary>
        /// Specify the environment to be used by the host.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
        /// <param name="environment">The environment to host the application in.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder UseEnvironment(this IHostBuilder hostBuilder, string environment)
            => hostBuilder.UseSetting(HostDefaults.EnvironmentKey, environment ?? throw new ArgumentNullException(nameof(environment)));

        public static IHostBuilder UseHostedService<THostedService>(this IHostBuilder hostBuilder)
                where THostedService : class, IHostedService
            => hostBuilder.ConfigureServices(services => services.AddHostedService<THostedService>());
    }
}
