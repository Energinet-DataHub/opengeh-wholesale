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

using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Common;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener;

public class MarketParticipantChangedListenerEndpoint
{
    private const string FunctionName = nameof(MarketParticipantChangedListenerEndpoint);

    private readonly ISharedIntegrationEventParser _sharedIntegrationEventParser;
    private readonly GridAreaUpdatedDtoFactory _gridAreaUpdatedDtoFactory;
    private readonly IJsonSerializer _jsonSerializer;

    public MarketParticipantChangedListenerEndpoint(
        ISharedIntegrationEventParser sharedIntegrationEventParser,
        GridAreaUpdatedDtoFactory gridAreaUpdatedDtoFactory,
        IJsonSerializer jsonSerializer)
    {
        _sharedIntegrationEventParser = sharedIntegrationEventParser;
        _gridAreaUpdatedDtoFactory = gridAreaUpdatedDtoFactory;
        _jsonSerializer = jsonSerializer;
    }

    [Function(FunctionName)]
    [EventHubOutput(
        "%" + EnvironmentSettingNames.IntegrationEventsEventHubName + "%",
        Connection = EnvironmentSettingNames.IntegrationEventsEventHubConnectionString)]
    public string? Run(
        byte[] message)
        //[ServiceBusTrigger(
        //    "%" + EnvironmentSettingNames.MarketParticipantChangedTopicName + "%",
        //    "%" + EnvironmentSettingNames.MarketParticipantChangedSubscriptionName + "%",
        //    Connection = EnvironmentSettingNames.IntegrationEventConnectionListenerString)]
    {
        var marketParticipantEvent = _sharedIntegrationEventParser.Parse(message);

        switch (marketParticipantEvent)
        {
            case GridAreaUpdatedIntegrationEvent gridAreaUpdatedIntegrationEvent:
                var gridAreaUpdatedDto = _gridAreaUpdatedDtoFactory.Create(gridAreaUpdatedIntegrationEvent);
                return _jsonSerializer.Serialize(gridAreaUpdatedDto);
            default:
                // Other types of events are irrelevant.
                return null;
        }
    }
}
