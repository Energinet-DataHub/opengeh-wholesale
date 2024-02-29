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
using Energinet.DataHub.Wholesale.Edi.Models;

namespace Energinet.DataHub.Wholesale.Edi.Mappers;

public static class CalculationTypeMapper
{
    /// <summary>
    /// Maps a <see cref="RequestedCalculationType"/> to a <see cref="CalculationType"/>. Cannot map <see cref="RequestedCalculationType.LatestCorrection"/> to a <see cref="CalculationType"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Throws a ArgumentOutOfRangeException if the request calculation type is unknown or has the value LatestCorrection</exception>
    public static CalculationType FromRequestedCalculationType(RequestedCalculationType requestedCalculationType)
    {
        if (requestedCalculationType == RequestedCalculationType.LatestCorrection)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedCalculationType),
                actualValue: requestedCalculationType,
                $"Value of type {nameof(RequestedCalculationType.LatestCorrection)} cannot be mapped to {nameof(CalculationType)}.");
        }

        return requestedCalculationType switch
        {
            RequestedCalculationType.BalanceFixing => CalculationType.BalanceFixing,
            RequestedCalculationType.PreliminaryAggregation => CalculationType.Aggregation,
            RequestedCalculationType.WholesaleFixing => CalculationType.WholesaleFixing,
            RequestedCalculationType.FirstCorrection => CalculationType.FirstCorrectionSettlement,
            RequestedCalculationType.SecondCorrection => CalculationType.SecondCorrectionSettlement,
            RequestedCalculationType.ThirdCorrection => CalculationType.ThirdCorrectionSettlement,
            _ => throw new ArgumentOutOfRangeException(
                nameof(requestedCalculationType),
                actualValue: requestedCalculationType,
                $"Value cannot be mapped to a {nameof(CalculationType)}."),
        };
    }
}
