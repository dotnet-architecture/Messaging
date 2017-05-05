// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Extensions.Messaging.RabbitMQ.Internal
{
    public class RabbitMQSubscription
    {
        private readonly List<object> _handlers = new List<object>();
        private readonly MethodInfo _invokeMethod;

        public RabbitMQSubscription(Type messageType)
        {
            MessageType = messageType;

            var funcType = typeof(Func<,,>).MakeGenericType(MessageType, typeof(CancellationToken), typeof(Task<bool>));
            _invokeMethod = funcType.GetMethod("Invoke");
        }

        public Type MessageType { get; }

        public void Add(object handler)
        {
            Guard.ArgumentNotNull(nameof(handler), handler);

            _handlers.Add(handler);
        }

        public async Task<bool> DispatchAsync(string messageBody, CancellationToken cancellationToken)
        {
            var message = JsonConvert.DeserializeObject(messageBody, MessageType);
            var tasks = new List<Task<bool>>();

            foreach (var handler in _handlers)
            {
                tasks.Add((Task<bool>)_invokeMethod.Invoke(handler, new object[] { message, cancellationToken }));
            }

            var results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }
    }
}
