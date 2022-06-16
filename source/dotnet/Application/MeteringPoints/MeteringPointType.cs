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

namespace Energinet.DataHub.Wholesale.Application.MeteringPoints
{
    /// <summary>
    /// WARNING !
    /// Do not change the numeric values since they are used further downstream for aggregation purposes.
    /// </summary>
    public enum MeteringPointType
    {
        Unknown = 0,
        Consumption = 1,
        Production = 2,
        Exchange = 3,
        VeProduction = 4,
        Analysis = 5,
        SurplusProductionGroup = 6,
        NetProduction = 7,
        SupplyToGrid = 8,
        ConsumptionFromGrid = 9,
        WholesaleService = 10,
        OwnProduction = 11,
        NetFromGrid = 12,
        NetToGrid = 13,
        TotalConsumption = 14,
        GridLossCorrection = 15,
        ElectricalHeating = 16,
        NetConsumption = 17,
        OtherConsumption = 18,
        OtherProduction = 19,
        ExchangeReactiveEnergy = 20,
        InternalUse = 21,
    }
}
