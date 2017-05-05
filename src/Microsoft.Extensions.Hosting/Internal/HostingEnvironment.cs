// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Extensions.Hosting.Internal
{
    public class HostingEnvironment : IApplicationEnvironment
    {
        public string ApplicationName { get; set; }

        public string EnvironmentName { get; set; } = Hosting.EnvironmentName.Production;
    }
}