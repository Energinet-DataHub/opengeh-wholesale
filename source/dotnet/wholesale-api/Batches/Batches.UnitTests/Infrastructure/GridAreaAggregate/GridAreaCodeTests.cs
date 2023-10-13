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

using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.GridAreaAggregate;

public class GridAreaCodeTests
{
    [Fact]
    public void Ctor_WhenNullArg_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GridAreaCode(null!));
    }

    [Theory]
    [InlineData("000")]
    [InlineData("001")]
    [InlineData("010")]
    [InlineData("100")]
    [InlineData("999")]
    public void Ctor_WhenZeroLeftPadded3DigitString_Creates(string codeString)
    {
        var actual = new GridAreaCode(codeString);
        actual.Code.Should().Be(codeString);
    }

    [Theory]
    [InlineData("0001")]
    [InlineData("10")]
    [InlineData("-100")]
    [InlineData("99e")]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_WhenNotZeroLeftPadded3DigitString_ThrowsBusinessValidationException(string codeString)
    {
        Assert.Throws<BusinessValidationException>(() => new GridAreaCode(codeString));
    }
}
