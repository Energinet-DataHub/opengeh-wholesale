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
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Application.Processes.Model;

public class ProcessMapperTests
{
    [Theory]
    [AutoData]
    public void Must_Map_From_Contract_ProcessType(ProcessTypeMapper sut)
    {
        // Arrange
        foreach (var type in Enum.GetValues(typeof(Batches.Interfaces.Models.ProcessType)))
        {
            var expected = (Batches.Interfaces.Models.ProcessType)type;

            // Act
            var actual = sut.MapFrom(expected);

            // Assert
            Assert.Equal(expected.ToString(), actual.ToString());
        }
    }

    [Theory]
    [AutoData]
    public void Must_Map_From_ProcessType(ProcessTypeMapper sut)
    {
        // Arrange
        foreach (var type in Enum.GetValues(typeof(ProcessType)))
        {
            var expected = (ProcessType)type;

            // Act
            var actual = sut.MapFrom(expected);

            // Assert
            Assert.Equal(expected.ToString(), actual.ToString());
        }
    }
}
