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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep.Model;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep;

/// <summary>
/// This class provides the ability to retrieve a calculated result for a given step for a batch.
/// </summary>
public interface IProcessStepApplicationService
{
    Task<WholesaleActorDto[]> GetEnergySuppliersAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType);

    Task<WholesaleActorDto[]> GetEnergySuppliersByBalanceResponsiblePartyAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsiblePartyGln);

    Task<WholesaleActorDto[]> GetBalanceResponsiblePartiesAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType);

    Task<ProcessStepResultDto> GetResultAsync(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? energySupplierGln,
        string? balanceResponsibleParty);
}
