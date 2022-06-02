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

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Json;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization
{
    public class JsonMessageSerializer : MessageSerializer
    {
        private readonly IJsonOutboundMapperFactory _mapperFactory;
        private readonly IJsonSerializer _jsonSerializer;

        public JsonMessageSerializer(IJsonOutboundMapperFactory mapperFactory, IJsonSerializer jsonSerializer)
        {
            _mapperFactory = mapperFactory;
            _jsonSerializer = jsonSerializer;
        }

        public override Task<byte[]> ToBytesAsync(IOutboundMessage message, CancellationToken cancellationToken = default)
        {
            var dto = GetDto(message);

            var result = SerializeDto(dto);

            return Task.FromResult(result);
        }

        private object GetDto(IOutboundMessage message)
        {
            var mapper = _mapperFactory.GetMapper(message);
            return mapper.Convert(message);
        }

        private byte[] SerializeDto(object dto)
        {
            var serializedDto = _jsonSerializer.Serialize(dto);

            return Encoding.UTF8.GetBytes(serializedDto);
        }
    }
}
