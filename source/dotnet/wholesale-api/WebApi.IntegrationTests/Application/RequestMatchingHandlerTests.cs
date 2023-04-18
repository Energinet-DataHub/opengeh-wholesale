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
using Energinet.DataHub.Wholesale.Application.Base;
using FluentAssertions;
using MediatR;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Application;

public class RequestMatchingHandlerTests
{
    /// <summary>
    /// A request can be either a command or a query.
    /// </summary>
    [Theory]
    [InlineAutoData(typeof(Wholesale.WebApi.Root))]
    [InlineAutoData(typeof(Wholesale.Application.Root))]
    [InlineAutoData(typeof(Wholesale.Domain.Root))]
    [InlineAutoData(typeof(Wholesale.Infrastructure.Root))]
    public void EachCommand_HasASingleHandler(Type type)
    {
        // Arrange
        var requestTypes = type.Assembly.GetTypes()
            .Where(IsIRequest)
            .ToList();

        var handlerTypes = type.Assembly.GetTypes()
            .Where(IsRequestHandler)
            .ToList();

        // Act
        foreach (var requestType in requestTypes)
        {
            // Assert
            handlerTypes.Should().ContainSingle(handlerType => IsHandlerForRequest(handlerType, requestType), $"handler for type {requestType} expected");
        }
    }

    private static bool IsIRequest(Type type)
    {
        // Ignore these interfaces as they don't have a handler
        if (type == typeof(ICommand) || type == typeof(ICommand<>) || type == typeof(IQuery<>))
        {
            return false;
        }

        return typeof(IBaseRequest).IsAssignableFrom(type);
    }

    private static bool IsRequestHandler(Type type)
    {
        return type.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType &&
                                                         (interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                                                          interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<>)));
    }

    private static bool IsHandlerForRequest(Type handlerType, Type requestType)
    {
        return handlerType.GetInterfaces().Any(i => i.GenericTypeArguments.Any(ta => ta == requestType));
    }
}
