// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Hosting.Internal
{
    public class HostOptions
    {
        public HostOptions()
        {
        }

        public HostOptions(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            ApplicationName = configuration[HostDefaults.ApplicationKey];
            DetailedErrors = ParseBool(configuration, HostDefaults.DetailedErrorsKey);
            Environment = configuration[HostDefaults.EnvironmentKey];
        }

        public string ApplicationName { get; set; }

        public bool DetailedErrors { get; set; }

        public string Environment { get; set; }

        public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);

        private static bool ParseBool(IConfiguration configuration, string key)
        {
            return string.Equals("true", configuration[key], StringComparison.OrdinalIgnoreCase)
                || string.Equals("1", configuration[key], StringComparison.OrdinalIgnoreCase);
        }
    }
}
