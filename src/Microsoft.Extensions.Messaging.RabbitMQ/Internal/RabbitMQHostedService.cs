// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.Messaging.RabbitMQ.Internal
{
    // REVIEW: This class seems mostly overhead, except that we can't really register a singleton
    // for two interface types (IHostedService _and_ IRabbitMQMessenger) without pre-creating
    // the instance.
    public class RabbitMQHostedService : IHostedService
    {
        private IRabbitMQMessenger _messenger;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMQHostedService(IServiceProvider serviceProvider)
        {
            // Since creation == initialization/connection, we delay retrieving the service
            // until we're asked to start.
            _serviceProvider = serviceProvider;
        }

        public void Start()
            => _messenger = _serviceProvider.GetRequiredService<IRabbitMQMessenger>();

        public void Stop()
            => _messenger?.Dispose();
    }
}
