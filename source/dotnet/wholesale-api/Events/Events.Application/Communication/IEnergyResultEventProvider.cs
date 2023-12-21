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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication
{
    public interface IEnergyResultEventProvider
    {
        /// <summary>
        /// Responsible for creating at least one <see cref="IntegrationEvent"/> for each <see cref="EnergyResult"/> available in <paramref name="calculation"/>.
        /// If we currently support multiple versions of an event then each result will cause multiple events to be provided.
        /// </summary>
        IAsyncEnumerable<IntegrationEvent> GetAsync(CompletedCalculation calculation);
    }
}
