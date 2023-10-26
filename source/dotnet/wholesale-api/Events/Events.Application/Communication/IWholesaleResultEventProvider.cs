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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication
{
    public interface IWholesaleResultEventProvider
    {
        /// <summary>
        /// Determines if <paramref name="batch"/> is in a state where it can
        /// contain <see cref="WholesaleResult"/>.
        /// </summary>
        /// <returns><langword>true</langword> if <paramref name="batch"/> can
        /// contain <see cref="WholesaleResult"/>; otherwise <langword>false</langword>.</returns>
        bool CanContainWholesaleResults(CompletedBatch batch);

        /// <summary>
        /// Responsible for creating an <see cref="IntegrationEvent"/> for each
        /// <see cref="WholesaleResult"/> available in <paramref name="batch"/>.
        /// </summary>
        IAsyncEnumerable<IntegrationEvent> GetAsync(CompletedBatch batch);
    }
}
