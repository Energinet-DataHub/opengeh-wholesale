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

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

public enum TimeSeriesType
{
    NonProfiledConsumption = 1,
    FlexConsumption = 2,
    Production = 3,
    NetExchangePerGa = 4,
    NetExchangePerNeighboringGa = 5,
    GridLoss = 6,
    NegativeGridLoss = 7,
    PositiveGridLoss = 8,
    TotalConsumption = 9,
    TempFlexConsumption = 10,
    TempProduction = 11,
}
