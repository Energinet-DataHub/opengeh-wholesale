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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.CalculationResultCompleted;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Mappers.CalculationResultCompleted;

public class ProcessTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(ProcessType.Aggregation, Contracts.Events.ProcessType.Aggregation)]
    [InlineAutoMoqData(ProcessType.BalanceFixing, Contracts.Events.ProcessType.BalanceFixing)]
    [InlineAutoMoqData(ProcessType.WholesaleFixing, Contracts.Events.ProcessType.WholesaleFixing)]
    [InlineAutoMoqData(ProcessType.FirstCorrectionSettlement, Contracts.Events.ProcessType.FirstCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.SecondCorrectionSettlement, Contracts.Events.ProcessType.SecondCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.ThirdCorrectionSettlement, Contracts.Events.ProcessType.ThirdCorrectionSettlement)]
    public void MapProcessType_WhenCalled_MapsCorrectly(ProcessType processType, Wholesale.Contracts.Events.ProcessType expected)
    {
        // Act & Assert
        ProcessTypeMapper.MapProcessType(processType).Should().Be(expected);
    }

    [Fact]
    public void MapProcessType_MapsAnyValidValue()
    {
        foreach (var processType in Enum.GetValues(typeof(ProcessType)).Cast<ProcessType>())
        {
            // Act
            var actual = ProcessTypeMapper.MapProcessType(processType);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(Contracts.Events.ProcessType), actual).Should().BeTrue();
        }
    }
}
