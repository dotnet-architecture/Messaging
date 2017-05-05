// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Extensions.Messaging.RabbitMQ
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RabbitMQMessageTypeAttribute : Attribute
    {
        public RabbitMQMessageTypeAttribute(string messageType)
            => MessageType = messageType;

        public string MessageType { get; }
    }
}
