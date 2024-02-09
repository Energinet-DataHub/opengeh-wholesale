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

using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Calculations.Application;

public interface ICalculationRepository
{
    Task AddAsync(Calculation calculation);

    Task<Calculation> GetAsync(Guid calculationId);

    Task<List<Calculation>> GetCreatedAsync();

    Task<List<Calculation>> GetByStatesAsync(IEnumerable<CalculationExecutionState> states);

    Task<List<Calculation>> GetCompletedAfterAsync(Instant? completedTime);

    Task<IReadOnlyCollection<Calculation>> SearchAsync(
        IReadOnlyCollection<GridAreaCode> filterByGridAreaCode,
        IReadOnlyCollection<CalculationExecutionState> filterByExecutionState,
        Instant? minExecutionTimeStart,
        Instant? maxExecutionTimeStart,
        Instant? periodStart,
        Instant? periodEnd,
        CalculationType? calculationType);
}
