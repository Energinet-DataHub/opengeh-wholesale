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

using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.Hosts;
using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.Database;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture;

/// <summary>
/// Fixture configuration used for integration tests running without TestCommon function app.
/// </summary>
[CollectionDefinition(nameof(ProcessManagerIntegrationTestHost))]
public sealed class ProcessManagerFixtureConfiguration :
    ICollectionFixture<ProcessManagerDatabaseFixture>,
    ICollectionFixture<ProcessManagerDatabricksFixture>
{
}
