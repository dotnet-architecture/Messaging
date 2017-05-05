// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting.Abstractions
{
    public interface IHost : IDisposable
    {
        /// <summary>
        /// The <see cref="IServiceProvider"/> for the host.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Starts listening on the configured addresses.
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Attempt to gracefully stop the host.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
