// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class HostExtensions
    {
        /// <summary>
        /// Starts the host.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static void Start(this IHost host)
        {
            host.StartAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Starts the host.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static Task StartAsync(this IHost host)
        {
            return host.StartAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gracefully stops the host.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static Task StopAsync(this IHost host)
        {
            return host.StopAsync(CancellationToken.None);
        }

        /// <summary>
        /// Attempts to gracefully stop the host with the given timeout.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="timeout">The timeout for stopping gracefully. Once expired the
        /// server may terminate any remaining active connections.</param>
        /// <returns></returns>
        public static Task StopAsync(this IHost host, TimeSpan timeout)
        {
            return host.StopAsync(new CancellationTokenSource(timeout).Token);
        }

        /// <summary>
        /// Runs a web application and block the calling thread until host shutdown.
        /// </summary>
        /// <param name="host">The <see cref="IHost"/> to run.</param>
        public static void Run(this IHost host)
        {
            host.RunAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs a web application and returns a Task that only completes on host shutdown.
        /// </summary>
        /// <param name="host">The <see cref="IHost"/> to run.</param>
        public static async Task RunAsync(this IHost host)
        {
            var done = new ManualResetEventSlim(false);
            using (var cts = new CancellationTokenSource())
            {
                Action shutdown = () =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        Console.WriteLine("Application is shutting down...");
                        try
                        {
                            cts.Cancel();
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }

                    done.Wait();
                };

                var assemblyLoadContext = AssemblyLoadContext.GetLoadContext(typeof(HostExtensions).GetTypeInfo().Assembly);
                assemblyLoadContext.Unloading += context => shutdown();

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    shutdown();
                    // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };

                await host.RunAsync(cts.Token, "Application started. Press Ctrl+C to shut down.");
                done.Set();
            }
        }

        /// <summary>
        /// Runs a web application and and returns a Task that only completes when the token is triggered or shutdown is triggered.
        /// </summary>
        /// <param name="host">The <see cref="IHost"/> to run.</param>
        /// <param name="token">The token to trigger shutdown.</param>
        public static Task RunAsync(this IHost host, CancellationToken token)
        {
            return host.RunAsync(token, shutdownMessage: null);
        }

        private static async Task RunAsync(this IHost host, CancellationToken token, string shutdownMessage)
        {
            using (host)
            {
                await host.StartAsync(token);

                var applicationEnvironment = host.Services.GetService<IApplicationEnvironment>();
                var hostLifetime = host.Services.GetService<IHostLifetime>();

                Console.WriteLine($"Hosting environment: {applicationEnvironment.EnvironmentName}");

                if (!string.IsNullOrEmpty(shutdownMessage))
                {
                    Console.WriteLine(shutdownMessage);
                }

                token.Register(state => ((IHostLifetime)state).StopApplication(), hostLifetime);

                var waitForStop = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                hostLifetime.ApplicationStopping.Register(obj =>
                {
                    var tcs = (TaskCompletionSource<object>)obj;
                    tcs.TrySetResult(null);
                }, waitForStop);

                await waitForStop.Task;

                // WebHost will use its default ShutdownTimeout if none is specified.
                await host.StopAsync();
            }
        }
    }
}
