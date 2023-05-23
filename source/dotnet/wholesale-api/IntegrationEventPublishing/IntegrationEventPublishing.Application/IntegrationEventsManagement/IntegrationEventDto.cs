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

using NodaTime;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.IntegrationEventsManagement;

public class IntegrationEventDto
{
    /// <summary>
    /// Integration Event data as an protobuf data (byte array).
    /// </summary>
    public byte[] EventData { get; }

    /// <summary>
    /// Public message type used by consumers to identify the type of the message being published. See also ADR-008.
    /// Example: CalculationResultCompleted.BalanceFixingEventName (BalanceFixingCalculationResultCompleted)
    /// </summary>
    public string MessageType { get; }

    /// <summary>
    /// The time of which the IntegrationEventDto was created.
    /// </summary>
    public Instant CreationDate { get; }

    public IntegrationEventDto(
        byte[] eventData,
        string messageType,
        Instant creationDate)
    {
        EventData = eventData;
        MessageType = messageType;
        CreationDate = creationDate;
    }

#pragma warning disable CS8618
    // Used in tests
    public IntegrationEventDto()
#pragma warning restore CS8618
    {
    }
}
