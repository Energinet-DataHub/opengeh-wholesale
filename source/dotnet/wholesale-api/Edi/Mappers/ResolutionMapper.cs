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

using Energinet.DataHub.Wholesale.Edi.Contracts;
using EnergyResultResolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Resolution;
using WholesaleResolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.Edi.Mappers;

public static class ResolutionMapper
{
    public static WholesaleResolution Map(string resolution)
    {
        return resolution switch
        {
            DataHubNames.Resolution.Hourly => WholesaleResolution.Hour,
            DataHubNames.Resolution.Daily => WholesaleResolution.Day,
            DataHubNames.Resolution.Monthly => WholesaleResolution.Month,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Unknown Resolution"),
        };
    }

    public static Energinet.DataHub.Edi.Responses.Resolution Map(EnergyResultResolution resolution)
    {
        return resolution switch
        {
            EnergyResultResolution.Hour => Energinet.DataHub.Edi.Responses.Resolution.Pt1H,
            EnergyResultResolution.Quarter => Energinet.DataHub.Edi.Responses.Resolution.Pt15M,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Unknown Resolution"),
        };
    }
}
