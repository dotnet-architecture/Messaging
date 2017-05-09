// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Extensions.Hosting.Internal
{
    internal static class LoggerEventIds
    {
        // REVIEW: I left these identical to ASP.NET Core (after pruning). Do they need to be?
        public const int Starting = 3;
        public const int Started = 4;
        public const int Shutdown = 5;
        public const int ApplicationStartupException = 6;
        public const int ApplicationStoppingException = 7;
        public const int ApplicationStoppedException = 8;
        public const int HostedServiceStartException = 9;
        public const int HostedServiceStopException = 10;
        public const int ServerShutdownException = 12;
    }
}
