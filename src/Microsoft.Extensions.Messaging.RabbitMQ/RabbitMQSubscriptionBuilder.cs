// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Messaging.RabbitMQ.Internal;
using RabbitMQ.Client;

namespace Microsoft.Extensions.Messaging.RabbitMQ
{
    public class RabbitMQSubscriptionBuilder
    {
        internal Dictionary<string, RabbitMQSubscription> Subscriptions { get; } = new Dictionary<string, RabbitMQSubscription>();

        internal Dictionary<Type, string> TypeNames = new Dictionary<Type, string>();

        internal IBasicConsumer CreateConsumer(ILogger logger, IModel model)
            => new RabbitMQBasicConsumer(this, logger, model);

        internal string GetMessageTypeName(Type messageType)
        {
            if (!TypeNames.TryGetValue(messageType, out var messageTypeName))
            {
                var messageTypeAttribute = messageType.GetTypeInfo().GetCustomAttribute<RabbitMQMessageTypeAttribute>();
                if (messageTypeAttribute != null)
                {
                    messageTypeName = messageTypeAttribute.MessageType;
                }
                else
                {
                    messageTypeName = messageType.Name;
                }

                TypeNames[messageType] = messageTypeName;
            }

            return messageTypeName;
        }

        public void Subscribe<TMessage>(Func<TMessage, CancellationToken, Task<bool>> handler)
        {
            Guard.ArgumentNotNull(nameof(handler), handler);

            var messageType = typeof(TMessage);
            var messageTypeName = GetMessageTypeName(messageType);
            if (!Subscriptions.TryGetValue(messageTypeName, out var subscription))
            {
                subscription = new RabbitMQSubscription(messageType);
                Subscriptions.Add(messageTypeName, subscription);
            }

            subscription.Add(handler);
        }
    }
}
