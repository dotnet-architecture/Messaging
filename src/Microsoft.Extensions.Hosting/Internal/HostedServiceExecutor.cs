// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.Internal
{
    public class HostedServiceExecutor
    {
        private readonly List<IHostedService> _services;
        private readonly ILogger<HostedServiceExecutor> _logger;

        public HostedServiceExecutor(ILogger<HostedServiceExecutor> logger, IEnumerable<IHostedService> services)
        {
            _logger = logger;
            _services = services.ToList();
        }

        public void Start()
        {
            if (_services.Count == 0)
            {
                throw new InvalidOperationException("No service for type 'Microsoft.Extensions.Hosting.IHostedService' has been registered.");
            }

            try
            {
                Execute(service => service.Start());
            }
            catch (Exception ex)
            {
                _logger.ApplicationError(LoggerEventIds.HostedServiceStartException, "An error occurred starting the application", ex);
            }
        }

        public void Stop()
        {
            try
            {
                Execute(service => service.Stop());
            }
            catch (Exception ex)
            {
                _logger.ApplicationError(LoggerEventIds.HostedServiceStopException, "An error occurred stopping the application", ex);
            }
        }

        private void Execute(Action<IHostedService> callback)
        {
            List<Exception> exceptions = null;

            foreach (var service in _services)
            {
                try
                {
                    callback(service);
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            // Throw an aggregate exception if there were any exceptions
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
