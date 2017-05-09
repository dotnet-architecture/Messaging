// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Messaging.RabbitMQ;
using Microsoft.Extensions.Messaging.RabbitMQ.Internal;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, Action<RabbitMQSubscriptionBuilder> builderAction)
        {
            var builder = new RabbitMQSubscriptionBuilder();
            builderAction(builder);

            services.AddSingleton<IRabbitMQMessenger>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<PersistentRabbitMQConnection>>();
                var settings = serviceProvider.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
                var connectionFactory = new ConnectionFactory
                {
                    HostName = settings.ServerName ?? "localhost",
                    Password = settings.Password,
                    UserName = settings.UserName,
                    VirtualHost = settings.VirtualHost ?? "/"
                };
                var connection = new PersistentRabbitMQConnection(connectionFactory, logger);

                var exchangeName = settings.ExchangeName ?? "RabbitMQMessenger";
                var queueName = settings.QueueName ?? Assembly.GetEntryAssembly().GetName().Name;
                return new RabbitMQMessenger(connection, builder, logger, exchangeName, queueName);
            });

            services.AddSingleton<IHostedService, RabbitMQHostedService>();
            return services;
        }
    }
}
