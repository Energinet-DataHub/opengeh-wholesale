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

using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.Wholesale.Edi;

/// <summary>
/// Handles specific messages sent to the WholesaleInbox service bus queue (typically received from the EDI subsystem)
/// </summary>
public interface IWholesaleInboxRequestHandler
{
    /// <summary>
    /// Get whether the handler can/should handle the incoming WholesaleInboxRequest type (the message subject)
    /// </summary>
    bool CanHandle(string requestSubject);

    /// <summary>
    /// Handles the process of consuming the WholesaleInboxRequest, then getting the required data and creating and sending the response.
    /// </summary>
    /// <param name="receivedMessage"></param>
    /// <param name="referenceId"></param>
    /// <param name="cancellationToken"></param>
    Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken);
}
