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

using System.Reflection;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Wholesale.Infrastructure.Core.MessagingExtensions.Serialization;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Energinet.DataHub.Wholesale.Infrastructure.Core.MessagingExtensions.Registration
{
    public static class ServiceCollectionProtobufExtensions
    {
        public static IServiceCollection ConfigureProtobufReception(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ProtobufInboundMapperFactory>();

            return serviceCollection;
        }

        /// <summary>
        /// Configure the <see cref="IServiceCollection"/> to handle proto buf
        /// </summary>
        /// <param name="services">service collection</param>
        /// <param name="configuration">Configuration of the parser</param>
        /// <typeparam name="TProtoContract">Protobuf contract</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <c>null</c></exception>
        public static IServiceCollection ReceiveProtobufMessage<TProtoContract>(
            this IServiceCollection services,
            Action<ProtobufOneOfConfiguration<TProtoContract>> configuration)
            where TProtoContract : class, IMessage
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var config = new ProtobufOneOfConfiguration<TProtoContract>();
            configuration.Invoke(config);

            services.AddScoped<MessageExtractor<TProtoContract>>();
            services.AddScoped<MessageDeserializer<TProtoContract>, ProtobufDeserializer<TProtoContract>>();

            foreach (var descriptor in ScanForMappers(typeof(TProtoContract).Assembly))
            {
                services.Add(descriptor);
            }

            services.AddScoped(_ => config.GetParser());

            return services;
        }

        /// <summary>
        /// Scan for mappers
        /// </summary>
        /// <param name="targetAssembly">Assembly to check</param>
        private static IEnumerable<ServiceDescriptor> ScanForMappers(Assembly targetAssembly)
        {
            var targetType = typeof(ProtobufInboundMapper<>);

            foreach (var type in targetAssembly.GetTypes())
            {
                if (type.BaseType == null)
                    continue;
                if (type.BaseType.IsGenericType == false)
                    continue;
                if (type.BaseType.GetGenericTypeDefinition() == targetType == false)
                    continue;

                var genericTypeParameter = type.BaseType.GenericTypeArguments[0];

                yield return new ServiceDescriptor(targetType.MakeGenericType(genericTypeParameter), type, ServiceLifetime.Scoped);
            }
        }
    }
}
