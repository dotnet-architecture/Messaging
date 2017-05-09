// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Extensions.Messaging.RabbitMQ
{
    public interface IRabbitMQMessenger : IDisposable
    {
        /// <summary>
        /// Publishes a message to the appropriate exchange.
        /// </summary>
        void Publish(object message);
    }
}
