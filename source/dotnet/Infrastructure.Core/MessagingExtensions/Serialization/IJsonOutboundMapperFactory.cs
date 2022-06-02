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

using Energinet.DataHub.Core.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization
{
    /// <summary>
    /// Interface for factory that can create a JSON mapper for outbound messages
    /// </summary>
    public interface IJsonOutboundMapperFactory
    {
        /// <summary>
        /// Retrieves the mapper needed for a specific outbound message
        /// </summary>
        /// <param name="message">The outbound message to find the mapper for</param>
        /// <returns>The mapper needed for the outbound message</returns>
        IJsonOutboundMapper GetMapper(IOutboundMessage message);
    }
}
