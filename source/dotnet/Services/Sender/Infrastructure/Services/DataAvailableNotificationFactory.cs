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

using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;

public class DataAvailableNotificationFactory : IDataAvailableNotificationFactory
{
    private const string CimDocumentType = "NotifyAggregatedMeasureData";

    public DataAvailableNotificationDto Create(ProcessCompletedEventDto completedProcessEvent)
    {
        var messageHubReference = Guid.NewGuid();
        var recipientGln = GetMdrGlnForGridArea(completedProcessEvent.GridAreaCode);

        // Unused when not supporting bundling
        var unusedRelativeWeight = 1;

        var notification = new DataAvailableNotificationDto(
            messageHubReference,
#pragma warning disable CS0618
            // This is a temporary solution which allow us to avoid integrating with the actor registry at the moment
            new LegacyActorIdDto(recipientGln),
#pragma warning restore CS0618
            new MessageTypeDto("process-completed"),
            CimDocumentType,
            DomainOrigin.Wholesale,
            supportsBundling: false,
            relativeWeight: unusedRelativeWeight);
        return notification;
    }

    private static string GetMdrGlnForGridArea(string gridAreaCode)
    {
        var gln = gridAreaCode switch
        {
            "805" => "8200000007739",
            "806" => "8200000007746",
            _ => throw new NotImplementedException("Only test grid areas 805 and 806 are supported."),
        };
        return gln;
    }
}
