// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Messaging.RabbitMQ;
using Microsoft.Extensions.Options;

namespace SimpleConsoleHostSample
{
    class MyMessage
    {
        public string Greeting { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    class Program
    {
        static Task<bool> GreetingHandler(MyMessage message, CancellationToken cancellationToken)
        {
            Console.WriteLine("[{0}] RECV: '{1}'", message.Timestamp, message.Greeting);
            return Task.FromResult(true);
        }

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    var settings = new RabbitMQSettings { ServerName = "192.168.80.129", UserName = "admin", Password = "Pass@word1" };

                    services.AddOptions()
                            .AddSingleton<IOptions<RabbitMQSettings>>(new OptionsWrapper<RabbitMQSettings>(settings))
                            .AddRabbitMQ(builder =>
                            {
                                builder.Subscribe<MyMessage>(GreetingHandler);
                            });
                })
                .Build();

            Console.WriteLine("Starting...");
            await host.StartAsync();

            var messenger = host.Services.GetRequiredService<IRabbitMQMessenger>();

            Console.WriteLine("Running. Type text and press ENTER to send a message.");

            Console.CancelKeyPress += async (sender, e) =>
            {
                Console.WriteLine("Shutting down...");
                await host.StopAsync(new CancellationTokenSource(3000).Token);
                Environment.Exit(0);
            };

            while (true)
            {
                var line = Console.ReadLine();

                if (!string.IsNullOrEmpty(line))
                {
                    var message = new MyMessage { Greeting = line, Timestamp = DateTimeOffset.Now };
                    messenger.Publish(message);
                }
            }
        }
    }
}
