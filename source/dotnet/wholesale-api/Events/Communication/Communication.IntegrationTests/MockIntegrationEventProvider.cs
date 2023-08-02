// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Microsoft.Extensions.Options;

namespace Communication.IntegrationTests;

internal class MockIntegrationEventProvider : IIntegrationEventProvider
{
    private readonly int _integrationEventsToProduce;

    public MockIntegrationEventProvider(IOptions<MockIntegrationEventProviderOptions> options)
    {
        _integrationEventsToProduce = options.Value.IntegrationEventsToProduce;
    }

    public IAsyncEnumerable<IntegrationEvent> GetAsync()
    {
        return Enumerable.Range(1, _integrationEventsToProduce)
            .Select(i => new IntegrationEvent(
                EventIdentification: Guid.NewGuid(),
                EventName: "Test",
                EventMinorVersion: 1,
                new Dummy() { Position = i }))
            .ToAsyncEnumerable();
    }
}
