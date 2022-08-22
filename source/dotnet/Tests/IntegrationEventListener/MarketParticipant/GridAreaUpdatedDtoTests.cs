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

using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Energinet.DataHub.Wholesale.Tests.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener.MarketParticipant;

[UnitTest]
public class GridAreaUpdatedDtoTests
{
    [Fact]
    public async Task PropertyNamesAndTypesMatchContractWithCalculator()
    {
        await using var stream = EmbeddedResources.GetStream("IntegrationEventListener.MarketParticipant.grid-area-updated.json");
        await ContractComplianceTestHelper.VerifyTypeCompliesWithContractAsync<GridAreaUpdatedDto>(stream);
    }
}
