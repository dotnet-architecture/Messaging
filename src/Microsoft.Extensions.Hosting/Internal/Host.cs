// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.Internal
{
    public class Host : IHost
    {
        private readonly IServiceCollection _applicationServiceCollection;
        private ApplicationLifetime _applicationLifetime;
        private HostedServiceExecutor _hostedServiceExecutor;

        private readonly IServiceProvider _hostingServiceProvider;
        private readonly HostOptions _options;
        private readonly IConfiguration _config;

        private IServiceProvider _applicationServices;
        private ILogger<Host> _logger;

        private bool _stopped;

        public Host(
            IServiceCollection appServices,
            IServiceProvider hostingServiceProvider,
            HostOptions options,
            IConfiguration config)
        {
            _applicationServiceCollection = appServices ?? throw new ArgumentNullException(nameof(appServices));
            _hostingServiceProvider = hostingServiceProvider ?? throw new ArgumentNullException(nameof(hostingServiceProvider));
            _options = options;
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _applicationServiceCollection.AddSingleton<IHostLifetime, ApplicationLifetime>();
            _applicationServiceCollection.AddSingleton<HostedServiceExecutor>();
        }

        public IServiceProvider Services
        {
            get
            {
                EnsureApplicationServices();
                return _applicationServices;
            }
        }

        public void Initialize()
        {
            EnsureApplicationServices();
        }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _logger = _applicationServices.GetRequiredService<ILogger<Host>>();
            _logger.Starting();

            Initialize();

            _applicationLifetime = _applicationServices.GetRequiredService<IHostLifetime>() as ApplicationLifetime;
            _hostedServiceExecutor = _applicationServices.GetRequiredService<HostedServiceExecutor>();
            var diagnosticSource = _applicationServices.GetRequiredService<DiagnosticSource>();

            // Fire IApplicationLifetime.Started
            _applicationLifetime?.NotifyStarted();

            // Fire IHostedService.Start
            _hostedServiceExecutor.Start();

            _logger.Started();

            return Task.CompletedTask;
        }

        private void EnsureApplicationServices()
        {
            if (_applicationServices == null)
            {
                _applicationServices = _applicationServiceCollection.BuildServiceProvider();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_stopped)
            {
                return Task.CompletedTask;
            }
            _stopped = true;

            _logger?.Shutdown();

            if (!cancellationToken.CanBeCanceled)
            {
                cancellationToken = new CancellationTokenSource(_options.ShutdownTimeout).Token;
            }

            // Fire IApplicationLifetime.Stopping
            _applicationLifetime?.StopApplication();

            // Fire the IHostedService.Stop
            _hostedServiceExecutor?.Stop();

            // Fire IApplicationLifetime.Stopped
            _applicationLifetime?.NotifyStopped();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (!_stopped)
            {
                try
                {
                    this.StopAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger?.ServerShutdownException(ex);
                }
            }

            (_applicationServices as IDisposable)?.Dispose();
            (_hostingServiceProvider as IDisposable)?.Dispose();
        }
    }
}
