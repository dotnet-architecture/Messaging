// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RabbitMQ.Client;
using System;

namespace Microsoft.Extensions.Messaging.RabbitMQ
{
    public interface IPersistentRabbitMQConnection : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
