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

using Energinet.DataHub.Wholesale.Common.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

public record EnergyResultQueryParameters(
    TimeSeriesType TimeSeriesType,
    Instant StartOfPeriod,
    Instant EndOfPeriod,
    string GridArea,
    string? EnergySupplierId,
    string? BalanceResponsibleId,
    ProcessType? ProcessType = null)
    //:// EnergyResultFilter(TimeSeriesType, StartOfPeriod, EndOfPeriod, GridArea, EnergySupplierId, BalanceResponsibleId)
{
    public EnergyResultQueryParameters(EnergyResultQueryParameters energyResultFilter, ProcessType processType)
        : this(
            energyResultFilter.TimeSeriesType,
            energyResultFilter.StartOfPeriod,
            energyResultFilter.EndOfPeriod,
            energyResultFilter.GridArea,
            energyResultFilter.EnergySupplierId,
            energyResultFilter.BalanceResponsibleId,
            processType) { }
}
