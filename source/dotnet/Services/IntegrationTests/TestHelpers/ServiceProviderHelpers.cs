// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestHelpers;

internal static class ServiceProviderHelpers
{
    /// <summary>
    /// Check if all needed types are registered in <see cref="IServiceProvider"/>
    /// </summary>
    /// <param name="services">All registered services</param>
    /// <param name="requirement">Requirement to be fulfilled</param>
    /// <returns>true if everything is registered, otherwise false</returns>
    public static bool CanSatisfyRequirement(this IServiceProvider services, Requirement requirement)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(requirement);

        return requirement.DependentOn.All(dependency => services.GetService(dependency) != null);
    }

    public static bool RequirementIsPartOfCollection<T>(this IServiceProvider serviceProvider, Requirement requirement)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(requirement);

        if (requirement.ActualType == null)
        {
            throw new InvalidOperationException(
                $"{nameof(requirement)} must have the property {nameof(requirement.ActualType)} set");
        }

        var collection = serviceProvider.GetServices<T>().ToLookup(t => t.GetType());
        return collection.Contains(requirement.ActualType);
    }
}
