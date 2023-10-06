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
using Energinet.DataHub.Wholesale.Batches.Application.Workers;
using Energinet.DataHub.Wholesale.Events.Application.Options;
using Energinet.DataHub.Wholesale.Events.Application.Triggers;
using Energinet.DataHub.Wholesale.WebApi.Configuration.Modules;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests;

public class RepeatingTriggerRegistrationTests
{
    [Theory]
    [InlineAutoData(typeof(RegisterCompletedBatchesTrigger))]
    [InlineAutoData(typeof(StartCalculationTrigger))]
    [InlineAutoData(typeof(UpdateBatchExecutionStateTrigger))]
    public void Repeating_Trigger_Is_Registered_In_Ioc(Type type, ServiceBusOptions options, ServiceCollection serviceCollection)
    {
        // Arrange
        serviceCollection.AddEventsModule(options);
        serviceCollection.AddBatchesModule(() => string.Empty);

        // Act
        var actual = serviceCollection.Count(x => x.ImplementationType == type);

        // Assert
        Assert.Equal(1, actual);
    }
}
