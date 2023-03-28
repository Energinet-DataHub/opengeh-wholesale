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

using AutoFixture.Xunit2;
using FluentAssertions;
using MediatR;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi;

public class RequestsRegistrationTests
{
    /// <summary>
    /// Code borrowed from https://matthiaslischka.at/2019/02/25/Testing-MediatR-Registrations/.
    /// </summary>
    [Theory]
    [InlineAutoData(typeof(Root))]
    [InlineAutoData(typeof(Application.Root))]
    [InlineAutoData(typeof(Domain.Root))]
    [InlineAutoData(typeof(Wholesale.Infrastructure.Root))]
    public void AllRequests_ShouldHaveMatchingHandler(Type type)
    {
        // Arrange
        var requestTypes = type.Assembly.GetTypes()
            .Where(IsRequest)
            .ToList();

        var handlerTypes = type.Assembly.GetTypes()
            .Where(IsIRequestHandler)
            .ToList();

        // Act & Assert
        foreach (var requestType in requestTypes) ShouldContainHandlerForRequest(handlerTypes, requestType);
    }

    private static void ShouldContainHandlerForRequest(IEnumerable<Type> handlerTypes, Type requestType)
    {
        handlerTypes.Should().ContainSingle(handlerType => IsHandlerForRequest(handlerType, requestType), $"Handler for type {requestType} expected");
    }

    private static bool IsRequest(Type type)
    {
        return typeof(IBaseRequest).IsAssignableFrom(type);
    }

    private static bool IsIRequestHandler(Type type)
    {
        return type.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
    }

    private static bool IsHandlerForRequest(Type handlerType, Type requestType)
    {
        return handlerType.GetInterfaces().Any(i => i.GenericTypeArguments.Any(ta => ta == requestType));
    }
}
