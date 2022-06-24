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

using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon
{
    /// <summary>
    /// Actually service bus listener mock extensions, but we want to separate the fluent API
    /// and make it stand out on its own.
    /// </summary>
    public static class WhenProviderExtensions
    {
        public static DoProvider WhenAny(this ServiceBusListenerMock provider)
        {
            return provider.When(_ => true);
        }

        public static DoProvider WhenCorrelationId(this ServiceBusListenerMock provider, string? correlationId = null)
        {
            return provider.When(request =>
                request.CorrelationId == correlationId);
        }
    }
}
