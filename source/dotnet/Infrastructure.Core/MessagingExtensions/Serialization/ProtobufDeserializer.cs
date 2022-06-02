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

using System;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization
{
    public class ProtobufDeserializer<TProtoContract> : MessageDeserializer<TProtoContract>
    {
        private readonly ProtobufInboundMapperFactory _inboundMapperFactory;
        private readonly ProtobufParser<TProtoContract> _parser;

        /// <summary>
        /// Create a <see cref="ProtobufMessageDeserializer"/>
        /// </summary>
        /// <param name="inboundMapperFactory"></param>
        /// <param name="parser"></param>
        public ProtobufDeserializer(
            ProtobufInboundMapperFactory inboundMapperFactory,
            ProtobufParser<TProtoContract> parser)
        {
            _inboundMapperFactory = inboundMapperFactory ?? throw new ArgumentNullException(nameof(inboundMapperFactory));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <inheritdoc cref="MessageDispatcher"/>
        public override Task<IInboundMessage> FromBytesAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            var dto = _parser.Parse(data);
            var mapper = _inboundMapperFactory.GetMapper(dto.GetType());
            var inboundMessage = mapper.Convert(dto);
            return Task.FromResult(inboundMessage);
        }
    }
}
