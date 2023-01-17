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

namespace Energinet.DataHub.Wholesale.Infrastructure.ServiceBus
{
    /// <summary>
    /// These constants represents the required meta data for integration events described here:
    /// https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/architecture-decision-record/ADR-0008%20Integration%20events.md
    /// </summary>
    public static class MessageMetaDataConstants
    {
        public const string OperationCorrelationId = "OperationCorrelationId";
        public const string OperationTimestamp = "OperationTimestamp";
        public const string MessageVersion = "MessageVersion";
        public const string MessageType = "MessageType";
        public const string EventIdentification = "EventIdentification";
    }
}
