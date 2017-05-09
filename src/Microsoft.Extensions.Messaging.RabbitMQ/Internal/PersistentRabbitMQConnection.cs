// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Microsoft.Extensions.Messaging.RabbitMQ.Internal
{
    class PersistentRabbitMQConnection : IPersistentRabbitMQConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<PersistentRabbitMQConnection> _logger;

        IConnection _connection;
        bool _disposed;

        object sync_root = new object();

        public PersistentRabbitMQConnection(IConnectionFactory connectionFactory, ILogger<PersistentRabbitMQConnection> logger)
        {
            Guard.ArgumentNotNull(nameof(connectionFactory), connectionFactory);
            Guard.ArgumentNotNull(nameof(logger), logger);

            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public bool IsConnected
            => !_disposed && _connection != null && _connection.IsOpen;

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            _logger.LogDebug("Attempting to connect to RabbitMQ");

            lock (sync_root)
            {
                var policy =
                    RetryPolicy.Handle<SocketException>()
                               .Or<BrokerUnreachableException>()
                               .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                               {
                                   _logger.LogWarning(ex.ToString());
                               });

                policy.Execute(() => _connection = _connectionFactory.CreateConnection());

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.LogDebug($"RabbitMQ connection established.");

                    return true;
                }
                else
                {
                    _logger.LogCritical("FATAL ERROR: RabbitMQ connection could not be established.");

                    return false;
                }
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogWarning("A RabbitMQ connection is blocked. Trying to re-connect...");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogWarning("A RabbitMQ connection threw an exception. Trying to re-connect...");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogWarning("A RabbitMQ connection has been shutdown. Trying to re-connect...");

            TryConnect();
        }
    }
}
