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

namespace Infrastructure.Core.MessagingExtensions.Serialization
{
    /// <summary>
    /// Interface for mapping an outbound message to a DTO that can be used for json serialization
    /// </summary>
    public interface IJsonOutboundMapper
    {
        /// <summary>
        /// Converts the outbound message into a serializable DTO
        /// </summary>
        /// <param name="message">Outbound message to convert</param>
        /// <returns>DTO that can be used in JSON serialization</returns>
        public object Convert(IOutboundMessage message);
    }
}
