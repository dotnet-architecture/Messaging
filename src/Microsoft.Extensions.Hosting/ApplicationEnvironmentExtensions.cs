// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationEnvironment"/>.
    /// </summary>
    public static class ApplicationEnvironmentExtensions
    {
        /// <summary>
        /// Checks if the current hosting environment name is "Development".
        /// </summary>
        /// <param name="applicationEnvironment">An instance of <see cref="IApplicationEnvironment"/>.</param>
        /// <returns>True if the environment name is "Development", otherwise false.</returns>
        public static bool IsDevelopment(this IApplicationEnvironment applicationEnvironment)
        {
            if (applicationEnvironment == null)
            {
                throw new ArgumentNullException(nameof(applicationEnvironment));
            }

            return applicationEnvironment.IsEnvironment(EnvironmentName.Development);
        }

        /// <summary>
        /// Checks if the current hosting environment name is "Staging".
        /// </summary>
        /// <param name="applicationEnvironment">An instance of <see cref="IApplicationEnvironment"/>.</param>
        /// <returns>True if the environment name is "Staging", otherwise false.</returns>
        public static bool IsStaging(this IApplicationEnvironment applicationEnvironment)
        {
            if (applicationEnvironment == null)
            {
                throw new ArgumentNullException(nameof(applicationEnvironment));
            }

            return applicationEnvironment.IsEnvironment(EnvironmentName.Staging);
        }

        /// <summary>
        /// Checks if the current hosting environment name is "Production".
        /// </summary>
        /// <param name="applicationEnvironment">An instance of <see cref="IApplicationEnvironment"/>.</param>
        /// <returns>True if the environment name is "Production", otherwise false.</returns>
        public static bool IsProduction(this IApplicationEnvironment applicationEnvironment)
        {
            if (applicationEnvironment == null)
            {
                throw new ArgumentNullException(nameof(applicationEnvironment));
            }

            return applicationEnvironment.IsEnvironment(EnvironmentName.Production);
        }

        /// <summary>
        /// Compares the current hosting environment name against the specified value.
        /// </summary>
        /// <param name="applicationEnvironment">An instance of <see cref="IApplicationEnvironment"/>.</param>
        /// <param name="environmentName">Environment name to validate against.</param>
        /// <returns>True if the specified name is the same as the current environment, otherwise false.</returns>
        public static bool IsEnvironment(this IApplicationEnvironment applicationEnvironment, string environmentName)
        {
            if (applicationEnvironment == null)
            {
                throw new ArgumentNullException(nameof(applicationEnvironment));
            }

            return string.Equals(applicationEnvironment.EnvironmentName, environmentName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
