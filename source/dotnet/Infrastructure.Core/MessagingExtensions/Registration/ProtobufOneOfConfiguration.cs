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

using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Wholesale.Infrastructure.Core.MessagingExtensions.Serialization;
using Google.Protobuf;

namespace Energinet.DataHub.Wholesale.Infrastructure.Core.MessagingExtensions.Registration
{
    public class ProtobufOneOfConfiguration<TOneOf>
    where TOneOf : class, IMessage
    {
        private Func<MessageParser> _getParser = GetMessageParserFromReflection;

        private Func<TOneOf, Enum>? _getMessageType;

        internal ProtobufOneOfConfiguration() { }

        /// <summary>
        /// Assign a parser
        /// </summary>
        /// <param name="parser">Func that returns a parser</param>
        public ProtobufOneOfConfiguration<TOneOf> WithParser(Func<MessageParser> parser)
        {
            _getParser = parser;
            return this;
        }

        /// <summary>
        /// Identifies the OneOf property from an envelope
        /// </summary>
        /// <param name="oneOfCase">Func used to return the value of the message type</param>
        /// <exception cref="InvalidOperationException">If <c>FromOneOf</c> has already been assigned</exception>
        /// <exception cref="ArgumentNullException"><paramref name="oneOfCase"/> is <c>null</c></exception>
        public ProtobufOneOfConfiguration<TOneOf> FromOneOf(Func<TOneOf, Enum> oneOfCase)
        {
            if (_getMessageType != null)
                throw new InvalidOperationException("OneOf already defined");
            _getMessageType = oneOfCase ?? throw new ArgumentNullException(nameof(oneOfCase));
            return this;
        }

        /// <summary>
        /// Configure a proto buf parser
        /// </summary>
        /// <returns><see cref="ProtobufParser"/> from the configuration</returns>
        internal ProtobufParser<TOneOf> GetParser()
        {
            return new ConfigParser<TOneOf>(_getParser.Invoke(), _getMessageType);
        }

        /// <summary>
        /// Locate a default parser based on convention
        /// </summary>
        /// <exception cref="InvalidOperationException">No property found with name 'Parser' or the property is not of type <see cref="MessageParser"/></exception>
        private static MessageParser GetMessageParserFromReflection()
        {
            var targetType = typeof(TOneOf);
            var propertyInfo = targetType.GetProperty("Parser");
            if (propertyInfo == null)
                throw new InvalidOperationException("No parser found on type " + targetType.Name);

            var parser = propertyInfo.GetValue(null) as MessageParser;

            return parser ?? throw new InvalidOperationException("Unable to get parser");
        }

        /// <summary>
        /// ProtobufParser implemented from configuration
        /// </summary>
        private sealed class ConfigParser<TProtobufContract> : ProtobufParser<TProtobufContract>
        {
            private readonly MessageParser _parser;
            private readonly Func<TOneOf, Enum>? _getMessageType;

            /// <summary>
            /// Create a parser
            /// </summary>
            /// <param name="parser">Protobuf parser to use</param>
            /// <param name="getMessageType"></param>
            public ConfigParser(MessageParser parser, Func<TOneOf, Enum>? getMessageType)
            {
                _parser = parser;
                _getMessageType = getMessageType;
            }

            /// <summary>
            /// Convert a payload to <see cref="IMessage"/>
            /// </summary>
            /// <param name="data">data to parser</param>
            /// <summary>
            /// Parse a byte array to an <see cref="IMessage"/>
            /// <exception cref="InvalidOperationException"> is returned if
            /// Data is not of the correct type - 'Invalid data'
            /// or
            /// A property is not located with a matching name - 'Invalid contract'
            /// or
            /// The property does not exists on the contract
            /// </exception>
            /// </summary>
            public override IMessage Parse(byte[] data)
            {
                if (_parser.ParseFrom(data) is not TOneOf envelope)
                    throw new InvalidOperationException("Invalid data");

                if (_getMessageType == null)
                    return envelope;

                var enumType = _getMessageType(envelope);
                var propertyName = Enum.GetName(enumType.GetType(), enumType);
                if (propertyName == null)
                    throw new InvalidOperationException("Invalid contract");

                var propertyInfo = envelope.GetType().GetProperty(propertyName);
                if (propertyInfo == null)
                    throw new InvalidOperationException();

                var message = propertyInfo.GetValue(envelope) as IMessage;

                return message ?? throw new InvalidOperationException();
            }
        }
    }
}
