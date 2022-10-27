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

using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using NodaTime;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
/// <summary>
/// Wholesales internal representation of the data on the EnergySupplierChanged event exposed by the market participant domain
/// </summary>
/// <param name="AccountingpointId">Unique metering point identification</param>
/// <param name="GsrnNumber">metering point identification</param>
/// <param name="EnergySupplierGln">Unique Energy Supplier identification</param>
/// <param name="EffectiveDate">Date which the change of supplier goes into effect</param>
/// <param name="Id">Unique event identification</param>
public sealed record EnergySupplierChangedDto(
        string AccountingpointId,
        string GsrnNumber,
        string EnergySupplierGln,
        Instant EffectiveDate,
        string Id,
        string CorrelationId,
        string MessageType,
        Instant OperationTime)
    : EventHubEventDtoBase(CorrelationId, MessageType, OperationTime);
