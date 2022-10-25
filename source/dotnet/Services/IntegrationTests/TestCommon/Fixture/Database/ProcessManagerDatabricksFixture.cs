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

using Energinet.DataHub.Wholesale.IntegrationTests.Components;
using Energinet.DataHub.Wholesale.ProcessManager;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.Database;

public sealed class ProcessManagerDatabricksFixture : IAsyncLifetime
{
    public ProcessManagerDatabricksFixture()
    {
        DatabricksManager = new DatabricksManager();
    }

    public DatabricksManager DatabricksManager { get; }

    public Task InitializeAsync()
    {
        DatabricksManager.BeginListen();

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl, DatabricksManager.DatabricksUrl);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceToken, DatabricksManager.DatabricksToken);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await DatabricksManager.DisposeAsync();
    }
}
