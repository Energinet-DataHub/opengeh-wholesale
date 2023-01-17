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
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon
{
    public static class ServiceBusReceivedMessageExtensions
    {
        /// <summary>
        /// Gets the OperationCorrelationId on the ServiceBusReceivedMessage from its ApplicationProperties. Throws an exception if the value is not set.
        /// </summary>
        /// <param name="serviceBusMessage"></param>
        public static string GetOperationCorrelationId(this ServiceBusReceivedMessage serviceBusMessage)
        {
            return serviceBusMessage.ApplicationProperties[MessageMetaDataConstants.OperationCorrelationId].ToString()!;
        }
    }
}
