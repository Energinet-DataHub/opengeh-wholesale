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

using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories
{
    public class CalculationResultIntegrationEventFactory : ICalculationResultIntegrationEventFactory
    {
        private readonly IClock _clock;
        private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;

        public CalculationResultIntegrationEventFactory(
            IClock clock,
            ICalculationResultCompletedFactory calculationResultCompletedFactory)
        {
            _clock = clock;
            _calculationResultCompletedFactory = calculationResultCompletedFactory;
        }

        public IntegrationEvent Create(CalculationResult calculationResult)
        {
            var @event = _calculationResultCompletedFactory.Create(calculationResult);
            return CreateIntegrationEvent(@event, calculationResult.Id);
        }

        private IntegrationEvent CreateIntegrationEvent(IMessage protobufMessage, Guid calculationResultId)
        {
            var eventIdentification = calculationResultId;
            var messageName = CalculationResultCompleted.MessageName;
            var messageVersion = CalculationResultCompleted.MessageVersion;
            var operationTimeStamp = _clock.GetCurrentInstant();
            return new IntegrationEvent(eventIdentification, messageName, operationTimeStamp, messageVersion, protobufMessage);
        }
    }
}
