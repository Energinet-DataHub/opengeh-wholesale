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

using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TotalMonthlyAmountResults;

public sealed class TotalMonthlyAmountResult
{
    public TotalMonthlyAmountResult(
        Guid id,
        Guid calculationId,
        CalculationType calculationType,
        Instant periodStart,
        Instant periodEnd,
        string gridArea,
        string energySupplierId,
        string? chargeOwnerId,
        decimal amount,
        long version)
    {
        Id = id;
        CalculationId = calculationId;
        CalculationType = calculationType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        GridArea = gridArea;
        EnergySupplierId = energySupplierId;
        ChargeOwnerId = chargeOwnerId;
        Amount = amount;
        Version = version;
    }

    public Guid Id { get; }

    public Guid CalculationId { get; set; }

    public CalculationType CalculationType { get; set; }

    public Instant PeriodStart { get; }

    public Instant PeriodEnd { get; }

    public string GridArea { get; }

    public string EnergySupplierId { get; }

    public string? ChargeOwnerId { get; }

    public decimal Amount { get; }

    public long Version { get; }
}
