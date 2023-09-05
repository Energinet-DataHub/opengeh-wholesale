﻿// Copyright 2020 Energinet DataHub A/S
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

namespace Energinet.DataHub.Wholesale.WebApi.Configuration.Options;

public class ServiceBusOptions
{
    /// <summary>
    /// Connection string to subscribe to the wholesale domain service bus queues and topics.
    /// </summary>
    public string SERVICE_BUS_SEND_CONNECTION_STRING { get; set; } = string.Empty;

    /// <summary>
    /// Connection string to manage the wholesale domain service bus namespace.
    /// </summary>
    public string SERVICE_BUS_MANAGE_CONNECTION_STRING { get; set; } = string.Empty;

    public string INTEGRATIONEVENTS_TOPIC_NAME { get; set; } = string.Empty;
}
