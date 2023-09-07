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

namespace Energinet.DataHub.Wholesale.Events.Application.InboxEvents;

public interface IAggregatedTimeSeriesMessageFactory
{
    /// <summary>
    /// Creates a service bus message based on aggregated time series
    /// </summary>
    /// <param name="aggregatedTimeSeries"></param>
    /// <param name="referenceId"></param>
    /// <param name="rejected">Temporary switch for generating accepted or rejected message</param>
    public ServiceBusMessage CreateResponse(List<object> aggregatedTimeSeries, string referenceId, bool rejected);

    /// <summary>
    /// Responsible for deserializing the received message.
    /// </summary>
    /// <param name="request"></param>
    AggregatedTimeSeriesRequest GetAggregatedTimeSeriesRequest(ServiceBusReceivedMessage request);
}
