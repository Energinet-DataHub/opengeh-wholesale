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

namespace Energinet.DataHub.Wholesale.WebApi.V3.Calculation;

public static class CalculationDtoMapper
{
    public static BatchDto Map(Calculations.Interfaces.Models.CalculationDto calculationDto)
    {
        if (calculationDto == null) throw new ArgumentNullException(nameof(calculationDto));

        return new BatchDto(
            calculationDto.RunId,
            calculationDto.BatchId,
            calculationDto.PeriodStart,
            calculationDto.PeriodEnd,
            calculationDto.Resolution,
            calculationDto.Unit,
            calculationDto.ExecutionTimeStart,
            calculationDto.ExecutionTimeEnd,
            CalculationStateMapper.MapState(calculationDto.ExecutionState),
            calculationDto.AreSettlementReportsCreated,
            calculationDto.GridAreaCodes,
            ProcessTypeMapper.Map(calculationDto.ProcessType),
            calculationDto.CreatedByUserId);
    }
}
