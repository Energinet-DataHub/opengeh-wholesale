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

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, "fake_value");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabaseConnectionString, "fake_value");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusSendConnectionString, "fake_value");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusManageConnectionString, "fake_value");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ProcessCompletedTopicName, "fake_value");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl, "fake_value");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceToken, "fake_value");

        // Act + Assert
        target.Build();
    }
}
