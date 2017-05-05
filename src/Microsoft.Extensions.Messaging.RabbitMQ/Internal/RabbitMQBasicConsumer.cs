// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Microsoft.Extensions.Messaging.RabbitMQ.Internal
{
    public class RabbitMQBasicConsumer : DefaultBasicConsumer
    {
        private readonly RabbitMQSubscriptionBuilder _builder;
        private readonly ILogger _logger;
        private readonly Policy _retryPolicy;

        public RabbitMQBasicConsumer(RabbitMQSubscriptionBuilder builder, ILogger logger, IModel model)
            : base(model)
        {
            _builder = builder;
            _logger = logger;
            _retryPolicy = Policy.Handle<SocketException>()
                                 .Or<OperationInterruptedException>()
                                 .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                                 {
                                     _logger.LogWarning(ex.ToString());
                                 });
        }

        public async Task DeliverAsync(ulong deliveryTag, string routingKey, byte[] body)
        {
            if (!_builder.Subscriptions.TryGetValue(routingKey, out var subscription))
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning($"Message with routing key '{routingKey}' received, but there are no listeners");
                    Model.BasicAck(deliveryTag, false);
                    return;
                }
            }

            var success = false;
            var requeue = true;

            try
            {
                // REVIEW: Should we introduce a timeout cancellation token?
                success = await subscription.DispatchAsync(Encoding.UTF8.GetString(body), CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception '{ex.GetType().FullName}' processing message for routing key '{routingKey}'");
                success = false;
                requeue = false;
            }

            _retryPolicy.Execute(() =>
            {
                if (success)
                {
                    Model.BasicAck(deliveryTag, false);
                }
                else
                {
                    // REVIEW: Do we want to support a dead letter queue?
                    Model.BasicNack(deliveryTag, false, requeue);
                }
            });
        }

        public override async void HandleBasicDeliver(
            string consumerTag,
            ulong deliveryTag,
            bool redelivered,
            string exchange,
            string routingKey,
            IBasicProperties properties,
            byte[] body)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Message received: deliveryTag = {deliveryTag}, redelivered = {redelivered}, exchange = '{exchange}', routing key = '{routingKey}'");
            }

            await DeliverAsync(deliveryTag, routingKey, body);
        }
    }
}
