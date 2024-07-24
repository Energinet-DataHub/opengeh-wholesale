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
using Energinet.DataHub.Core.Messaging.Communication;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication.Messaging;

//// TODO - XDAST: Currently refactoring, so this is a step on the way. Code was copied from the Messaging package.

/// <summary>
/// Creates a <see cref="ServiceBusMessage"/> instance from an <see cref="IntegrationEvent"/>
/// </summary>
public interface IServiceBusMessageFactory
{
    ServiceBusMessage Create(IntegrationEvent @event);
}
