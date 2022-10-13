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

using Energinet.DataHub.Wholesale.ProcessManager;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Hosts;

[UnitTest]
public sealed class ProcessManagerHostTests
{
    [Fact]
    public void When_HostIsBuilt_Then_AllServicesMustBeRegistered()
    {
        // Arrange
        var target = Program
            .BuildAppHost()
            .UseDefaultServiceProvider(config => config.ValidateOnBuild = true);

        const string placeholderValue = "placeholder_value";

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, placeholderValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabaseConnectionString, placeholderValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusSendConnectionString, "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusManageConnectionString, placeholderValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventsTopicName, placeholderValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ZipBasisDataWhenCompletedBatchSubscriptionName, placeholderValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.PublishProcessesCompletedWhenCompletedBatchSubscriptionName, placeholderValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl, placeholderValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceToken, placeholderValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString, "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageContainerName, placeholderValue);

        // Act + Assert
        target.Build();
    }
}
