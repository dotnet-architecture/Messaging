// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class HostingServiceCollectionExtensions
    {
        public static IServiceCollection AddHostedService<THostedService>(this IServiceCollection serviceCollection)
                where THostedService : class, IHostedService
            => serviceCollection.AddSingleton<IHostedService, THostedService>();
    }
}
