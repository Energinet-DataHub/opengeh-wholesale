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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.Configuration.Options;

public class OptionsTests
{
    [Theory]
    [InlineAutoMoqData(typeof(DataLakeOptions), 4, "STORAGE_ACCOUNT_URI", "STORAGE_CONTAINER_NAME", "DATALAKE_HEALTH_CHECK_START", "DATALAKE_HEALTH_CHECK_END")]
    [InlineAutoMoqData(typeof(ConnectionStringsOptions), 1, "DB_CONNECTION_STRING")]
    [InlineAutoMoqData(typeof(DeltaTableOptions), 5, "SCHEMA_NAME", "BasisDataSchemaName", "SettlementReportSchemaName", "ENERGY_RESULTS_TABLE_NAME", "WHOLESALE_RESULTS_TABLE_NAME")]
    public void Options_HaveTheCorrectSettingNamesAndNumberOfSettings(Type sut, int settingsCount, params string[] expectedNames)
    {
        // Arrange & Act
        var properties = sut.GetProperties();

        // Assert
        properties.Length.Should().Be(settingsCount, $"the type {sut.Name}.");
        properties.Length.Should().Be(expectedNames.Length);
        foreach (var property in properties)
        {
            property.Name.Should().BeOneOf(expectedNames);
        }
    }

    [Theory]
    [InlineAutoMoqData(typeof(ConnectionStringsOptions), 1, "CONNECTIONSTRINGS")]
    public void Options_HaveTheCorrectSectionNames(Type sut, int numberOfSections, params string[] expectedNames)
    {
        // Arrange & Act
        var members = sut.GetMembers(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static);

        // Assert
        numberOfSections.Should().Be(members.Length);
        foreach (var member in members)
        {
            var actualName = ((FieldInfo)member)
                .GetValue(sut)!
                .ToString();
            actualName.Should().BeOneOf(expectedNames);
        }
    }
}
