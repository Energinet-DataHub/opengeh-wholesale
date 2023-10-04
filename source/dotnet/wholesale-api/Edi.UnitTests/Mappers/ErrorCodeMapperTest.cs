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

using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.EDI.Mappers;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Mappers;

public class ErrorCodeMapperTest
{
    [Theory]
    [InlineData("D66", ErrorCodes.InvalidPeriod)]
    public void MapErrorCode_StringToProtobufErrorCode_ReturnsExpectedErrorCode(string errorCode, ErrorCodes expected)
    {
        // Act
        var actual = ErrorCodeMapper.MapErrorCode(errorCode);

        // Assert
        actual.Should().Be(expected);
    }
}
