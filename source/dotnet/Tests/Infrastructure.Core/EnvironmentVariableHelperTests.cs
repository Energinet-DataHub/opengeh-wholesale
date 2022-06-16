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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.Core
{
    public class EnvironmentVariableHelperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void GetEnvVariable_HasVariable_ReturnsVariable(string name, string expected)
        {
            // Arrange
            Environment.SetEnvironmentVariable(name, expected);

            // Act
            var actual = EnvironmentVariableHelper.GetEnvVariable(name);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void GetEnvVariable_WhenNotHasVariable_ThrowsException(string name)
        {
            try
            {
                // Arrange + Act
                var actual = EnvironmentVariableHelper.GetEnvVariable(name);
                actual.Should().Be("must_not_happen");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Message.Should().Contain(name);
            }
        }
    }
}
