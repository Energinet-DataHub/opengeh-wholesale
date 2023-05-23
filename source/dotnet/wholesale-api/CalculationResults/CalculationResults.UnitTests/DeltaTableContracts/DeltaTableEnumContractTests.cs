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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.DeltaTableContracts;

public class DeltaTableEnumContractTests
{
    // TODO BJM: How about aggregation level, metering point type and settlement method?
    [Fact]
    public async Task ProcessType_Matches_Contract()
    {
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.process-type.json");
        await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<ProcessType>(stream);
    }

    [Fact]
    public async Task QuantityQuality_Matches_Contract()
    {
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.quantity-quality.json");
        await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<QuantityQuality>(stream);
    }

    [Fact]
    public async Task TimeSeriesType_Matches_Contract()
    {
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.time-series-type.json");
        await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<TimeSeriesType>(stream);
    }
}
