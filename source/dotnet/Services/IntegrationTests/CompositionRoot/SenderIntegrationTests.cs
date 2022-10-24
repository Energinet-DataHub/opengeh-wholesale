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

using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using Energinet.DataHub.Wholesale.IntegrationTests.TestHelpers;
using Energinet.DataHub.Wholesale.Sender;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationTests.CompositionRoot;

public class SenderIntegrationTests
{
    #region Member data providers
    public static IEnumerable<object[]> GetFunctionRequirements()
    {
        var allTypes = ReflectionHelper.FindAllTypes();
        var functionTypes = ReflectionHelper.FindAllFunctionTypes();
        var constructorDependencies = ReflectionHelper.FindAllConstructorDependenciesForType();

        return functionTypes(allTypes(typeof(Program)))
            .Select(f => new object[] { new Requirement(f.Name, constructorDependencies(f)) });
    }
    #endregion

    [Theory(DisplayName = nameof(All_dependencies_can_be_resolved_in_function))]
    [MemberData(nameof(GetFunctionRequirements))]
    public async Task All_dependencies_can_be_resolved_in_function(Requirement requirement)
    {
        using var host = await SenderIntegrationTestHost.CreateAsync();
        await using var scope = host.BeginScope();
        Assert.True(scope.ServiceProvider.CanSatisfyRequirement(requirement));
    }
}
