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

using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;

public class CompletedCalculationFactory : ICompletedCalculationFactory
{
    public IEnumerable<CompletedCalculation> CreateFromCalculations(IEnumerable<CalculationDto> completedCalculationDtos)
    {
        return completedCalculationDtos.Select(dto => CreateFromCalculation(dto, orchestrationInstanceId: null));
    }

    public CompletedCalculation CreateFromCalculation(CalculationDto calculation, string? orchestrationInstanceId)
    {
        if (calculation.ExecutionTimeEnd == null)
            throw new ArgumentNullException($"{nameof(CalculationDto.ExecutionTimeEnd)} should not be null for a completed calculation.");

        return new CompletedCalculation(
            calculation.CalculationId,
            calculation.GridAreaCodes.ToList(),
            calculation.CalculationType,
            calculation.PeriodStart.ToInstant(),
            calculation.PeriodEnd.ToInstant(),
            completedTime: calculation.ExecutionTimeEnd.Value.ToInstant(),
            calculation.Version,
            orchestrationInstanceId);
    }
}
