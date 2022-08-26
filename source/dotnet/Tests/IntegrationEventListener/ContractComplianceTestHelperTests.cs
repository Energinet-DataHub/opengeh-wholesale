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

using System.Text;
using Energinet.DataHub.Wholesale.Tests.TestHelpers;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener;

[UnitTest]
public class ContractComplianceTestHelperTests
{
    [Fact]
    public async Task GetRequiredMessageTypeAsync_GivenMessageType_ReturnsMessageType()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""MessageType"",
          ""type"": ""string"",
          ""value"": ""TestMessageType""
        }
    ]
}";

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act
        var actual = await ContractComplianceTestHelper.GetRequiredMessageTypeAsync(contractStream);

        // Assert
        actual.Should().Be("TestMessageType");
    }

    [Fact]
    public async Task VerifyTypeCompliesWithContractAsync_GivenProperty_VerifiesPropertyExists()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""ExpectedProperty"",
          ""type"": ""string""
        }
    ]
}";

        var targetObject = new
        {
            ExpectedProperty = "contents",
        };

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        await VerifyObjectCompliesWithContractAsync(targetObject, contractStream);
    }

    [Fact]
    public async Task VerifyTypeCompliesWithContractAsync_WrongProperty_ThrowsException()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""UnrelatedProperty"",
          ""type"": ""string""
        }
    ]
}";

        var targetObject = new
        {
            ExpectedProperty = "contents",
        };

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        try
        {
            await VerifyObjectCompliesWithContractAsync(targetObject, contractStream);
            Assert.True(false);
        }
        catch
        {
            // Non-compliant object fails.
        }
    }

    [Fact]
    public async Task VerifyTypeCompliesWithContractAsync_WrongPropertyType_ThrowsException()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""ExpectedProperty"",
          ""type"": ""integer""
        }
    ]
}";

        var targetObject = new
        {
            ExpectedProperty = "contents",
        };

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        try
        {
            await VerifyObjectCompliesWithContractAsync(targetObject, contractStream);
            Assert.True(false);
        }
        catch
        {
            // Non-compliant object fails.
        }
    }

    [Fact]
    public async Task VerifyTypeCompliesWithContractAsync_IntegerPropertyType_VerifiesProperty()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""ExpectedProperty"",
          ""type"": ""integer""
        }
    ]
}";

        var targetObject = new
        {
            ExpectedProperty = 10,
        };

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        await VerifyObjectCompliesWithContractAsync(targetObject, contractStream);
    }

    [Fact]
    public async Task VerifyTypeCompliesWithContractAsync_TimestampPropertyType_VerifiesProperty()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""ExpectedProperty"",
          ""type"": ""timestamp""
        }
    ]
}";

        var targetObject = new
        {
            ExpectedProperty = NodaTime.Instant.MinValue,
        };

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        await VerifyObjectCompliesWithContractAsync(targetObject, contractStream);
    }

    [Fact]
    public async Task VerifyTypeCompliesWithContractAsync_GuidPropertyType_VerifiesProperty()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""ExpectedProperty"",
          ""type"": ""string""
        }
    ]
}";

        var targetObject = new
        {
            ExpectedProperty = Guid.Empty,
        };

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        await VerifyObjectCompliesWithContractAsync(targetObject, contractStream);
    }

    [Fact]
    public async Task VerifyTypeCompliesWithContractAsync_NullablePropertyType_VerifiesProperty()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""ExpectedProperty"",
          ""type"": ""integer""
        }
    ]
}";

        var targetObject = new
        {
            ExpectedProperty = (int?)null,
        };

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        await VerifyObjectCompliesWithContractAsync(targetObject, contractStream);
    }

    [Fact]
    public async Task VerifyTypeCompliesWithContractAsync_EnumPropertyType_VerifiesProperty()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""ExpectedProperty"",
          ""type"": ""integer""
        }
    ]
}";

        var targetObject = new
        {
            ExpectedProperty = DayOfWeek.Monday,
        };

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        await VerifyObjectCompliesWithContractAsync(targetObject, contractStream);
    }

    [Fact]
    public async Task VerifyTypeCompliesWithContractAsync_IncorrectNumberOfProperties_ThrowsException()
    {
        // Arrange
        const string contract = @"
{
    ""fields"":
    [
        {
          ""name"": ""ExpectedProperty"",
          ""type"": ""integer""
        }
    ]
}";

        var targetObject = new
        {
            ExpectedProperty = DayOfWeek.Monday,
            UnexpectedProperty = DayOfWeek.Monday,
        };

        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        try
        {
            await VerifyObjectCompliesWithContractAsync(targetObject, contractStream);
            Assert.True(false);
        }
        catch
        {
            // Non-compliant object fails.
        }
    }

    [Fact]
    public async Task VerifyEnumCompliesWithContractAsync_GivenMatchingContract_VerifiesEnum()
    {
        // Arrange
        const string contract = @"
{
    ""literals"":
    [
        {
          ""name"": ""LitA"",
          ""value"": 2
        },
        {
          ""name"": ""LitB"",
          ""value"": 5
        }
    ]
}";
        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<TestEnum>(contractStream);
    }

    [Fact]
    public async Task VerifyEnumCompliesWithContractAsync_TooFewLiterals_ThrowsException()
    {
        // Arrange
        const string contract = @"
{
    ""literals"":
    [
        {
          ""name"": ""LitA"",
          ""value"": 2
        }
    ]
}";
        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        try
        {
            await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<TestEnum>(contractStream);
            Assert.True(false);
        }
        catch
        {
            // Non-compliant object fails.
        }
    }

    [Fact]
    public async Task VerifyEnumCompliesWithContractAsync_TooManyLiterals_ThrowsException()
    {
        // Arrange
        const string contract = @"
{
    ""literals"":
    [
        {
          ""name"": ""LitA"",
          ""value"": 2
        },
        {
          ""name"": ""LitB"",
          ""value"": 5
        },
        {
          ""name"": ""LitC"",
          ""value"": 10
        }
    ]
}";
        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        try
        {
            await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<TestEnum>(contractStream);
            Assert.True(false);
        }
        catch
        {
            // Non-compliant object fails.
        }
    }

    [Fact]
    public async Task VerifyEnumCompliesWithContractAsync_LiteralsIncorrectName_ThrowsException()
    {
        // Arrange
        const string contract = @"
{
    ""literals"":
    [
        {
          ""name"": ""LitA"",
          ""value"": 2
        },
        {
          ""name"": ""LitD"",
          ""value"": 5
        }
    ]
}";
        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        try
        {
            await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<TestEnum>(contractStream);
            Assert.True(false);
        }
        catch
        {
            // Non-compliant object fails.
        }
    }

    [Fact]
    public async Task VerifyEnumCompliesWithContractAsync_LiteralsIncorrectValue_ThrowsException()
    {
        // Arrange
        const string contract = @"
{
    ""literals"":
    [
        {
          ""name"": ""LitA"",
          ""value"": 7
        },
        {
          ""name"": ""LitB"",
          ""value"": 5
        }
    ]
}";
        using var contractStream = new MemoryStream(Encoding.UTF8.GetBytes(contract));

        // Act + Assert
        try
        {
            await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<TestEnum>(contractStream);
            Assert.True(false);
        }
        catch
        {
            // Non-compliant object fails.
        }
    }

    private static Task VerifyObjectCompliesWithContractAsync<T>(T inferType, Stream contractStream)
    {
        return ContractComplianceTestHelper.VerifyTypeCompliesWithContractAsync<T>(contractStream);
    }

    private enum TestEnum
    {
        LitA = 2,
        LitB = 5,
    }
}
