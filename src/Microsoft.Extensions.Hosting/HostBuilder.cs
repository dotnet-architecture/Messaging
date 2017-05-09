// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// A builder for <see cref="IHost"/>
    /// </summary>
    public class HostBuilder : IHostBuilder
    {
        private readonly IApplicationEnvironment _hostingEnvironment;
        private readonly List<Action<IServiceCollection>> _configureServicesDelegates;
        private readonly List<Action<ILoggerFactory>> _configureLoggingDelegates;

        private IConfiguration _config;
        private ILoggerFactory _loggerFactory;
        private HostOptions _options;
        private bool _hostBuilt;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBuilder"/> class.
        /// </summary>
        public HostBuilder()
        {
            _hostingEnvironment = new HostingEnvironment();
            _configureServicesDelegates = new List<Action<IServiceCollection>>();
            _configureLoggingDelegates = new List<Action<ILoggerFactory>>();

            _config = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "DOTNETCORE_")
                .Build();
        }

        /// <summary>
        /// Add or replace a setting in the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to add or replace.</param>
        /// <param name="value">The value of the setting to add or replace.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        public IHostBuilder UseSetting(string key, string value)
        {
            _config[key] = value;
            return this;
        }

        /// <summary>
        /// Get the setting value from the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to look up.</param>
        /// <returns>The value the setting currently contains.</returns>
        public string GetSetting(string key)
        {
            return _config[key];
        }

        /// <summary>
        /// Specify the <see cref="ILoggerFactory"/> to be used by the host.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to be used.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        public IHostBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            return this;
        }

        /// <summary>
        /// Adds a delegate for configuring additional services for the host. This may be called
        /// multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        public IHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            _configureServicesDelegates.Add(configureServices ?? throw new ArgumentNullException(nameof(configureServices)));
            return this;
        }

        /// <summary>
        /// Adds a delegate for configuring the provided <see cref="ILoggerFactory"/>. This may be called multiple times.
        /// </summary>
        /// <param name="configureLogging">The delegate that configures the <see cref="ILoggerFactory"/>.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        public IHostBuilder ConfigureLogging(Action<ILoggerFactory> configureLogging)
        {
            _configureLoggingDelegates.Add(configureLogging ?? throw new ArgumentNullException(nameof(configureLogging)));
            return this;
        }

        /// <summary>
        /// Builds the required services and an <see cref="IHost"/> which hosts an application.
        /// </summary>
        public IHost Build()
        {
            if (_hostBuilt)
            {
                throw new InvalidOperationException(Resources.HostBuilder_SingleInstance);
            }
            _hostBuilt = true;

            var hostingServices = BuildCommonServices();
            var applicationServices = hostingServices.Clone();
            var hostingServiceProvider = hostingServices.BuildServiceProvider();

            AddApplicationServices(applicationServices, hostingServiceProvider);

            var host = new Host(applicationServices, hostingServiceProvider, _options, _config);

            host.Initialize();

            return host;
        }

        private IServiceCollection BuildCommonServices()
        {
            _options = new HostOptions(_config);

            var applicationName = _options.ApplicationName ?? Assembly.GetEntryAssembly().GetName().Name;

            _hostingEnvironment.Initialize(applicationName, _options);

            var services = new ServiceCollection();
            services.AddSingleton(_hostingEnvironment);

            if (_loggerFactory == null)
            {
                _loggerFactory = new LoggerFactory();
            }

            services.AddSingleton(_loggerFactory);

            foreach (var configureLogging in _configureLoggingDelegates)
            {
                configureLogging(_loggerFactory);
            }

            services.AddLogging();

            var listener = new DiagnosticListener("Microsoft.DotNetCore");
            services.AddSingleton<DiagnosticListener>(listener);
            services.AddSingleton<DiagnosticSource>(listener);

            services.AddOptions();

            services.AddTransient<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();

            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            foreach (var configureServices in _configureServicesDelegates)
            {
                configureServices(services);
            }

            return services;
        }

        private void AddApplicationServices(IServiceCollection services, IServiceProvider hostingServiceProvider)
        {
            // We are forwarding services from hosting contrainer so hosting container
            // can still manage their lifetime (disposal) shared instances with application services.
            // NOTE: This code overrides original services lifetime. Instances would always be singleton in
            // application container.
            var loggerFactory = hostingServiceProvider.GetService<ILoggerFactory>();
            services.Replace(ServiceDescriptor.Singleton(typeof(ILoggerFactory), loggerFactory));

            var listener = hostingServiceProvider.GetService<DiagnosticListener>();
            services.Replace(ServiceDescriptor.Singleton(typeof(DiagnosticListener), listener));
            services.Replace(ServiceDescriptor.Singleton(typeof(DiagnosticSource), listener));
        }
    }
}
