// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Extensions.Messaging.RabbitMQ
{
    public class RabbitMQSettings
    {
        public string ExchangeName { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string VirtualHost { get; set; }
    }
}
