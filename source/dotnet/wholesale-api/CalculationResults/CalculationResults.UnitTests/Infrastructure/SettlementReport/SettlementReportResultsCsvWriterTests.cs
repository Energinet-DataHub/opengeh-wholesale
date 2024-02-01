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

using System.Globalization;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;

public class SettlementReportResultsCsvWriterTests
{
    private static readonly CultureInfo _testCulture = new("da-DK");
    private static readonly SettlementReportResultRow _validRow = new(
        "500",
        CalculationType.BalanceFixing,
        Instant.FromUtc(2021, 1, 1, 0, 0),
        "PT15M",
        MeteringPointType.Consumption,
        SettlementMethod.Flex,
        1000.521m);

    [Fact]
    public static async Task WriteAsync_GivenTwoRows_WritesValidDaDkCsv()
    {
        // Arrange
        await using var memoryStream = new MemoryStream();
        var sut = new SettlementReportResultsCsvWriter();

        var rows = new[]
        {
            _validRow,
            _validRow,
        };

        // Act
        await sut.WriteAsync(memoryStream, rows, new CultureInfo("da-DK"));

        // Assert
        var text = await ReadStreamAsStringAsync(memoryStream);
        var lines = text.Split(Environment.NewLine);

        Assert.Equal("METERINGGRIDAREAID;ENERGYBUSINESSPROCESS;STARTDATETIME;RESOLUTIONDURATION;TYPEOFMP;SETTLEMENTMETHOD;ENERGYQUANTITY", lines[0]);
        Assert.Equal("500;D04;2021-01-01T00:00:00Z;PT15M;E17;D01;1000,521", lines[1]);
        Assert.Equal("500;D04;2021-01-01T00:00:00Z;PT15M;E17;D01;1000,521", lines[2]);
    }

    [Fact]
    public static async Task WriteAsync_GivenTwoRows_WritesValidEnUsCsv()
    {
        // Arrange
        await using var memoryStream = new MemoryStream();
        var sut = new SettlementReportResultsCsvWriter();

        var rows = new[]
        {
            _validRow,
            _validRow,
        };

        // Act
        await sut.WriteAsync(memoryStream, rows, new CultureInfo("en-US"));

        // Assert
        var text = await ReadStreamAsStringAsync(memoryStream);
        var lines = text.Split(Environment.NewLine);

        Assert.Equal("METERINGGRIDAREAID,ENERGYBUSINESSPROCESS,STARTDATETIME,RESOLUTIONDURATION,TYPEOFMP,SETTLEMENTMETHOD,ENERGYQUANTITY", lines[0]);
        Assert.Equal("500,D04,2021-01-01T00:00:00Z,PT15M,E17,D01,1000.521", lines[1]);
        Assert.Equal("500,D04,2021-01-01T00:00:00Z,PT15M,E17,D01,1000.521", lines[2]);
    }

    [Fact]
    public static async Task WriteAsync_GivenValidRow_WritesCorrectlyFormattedGridArea()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var sut = new SettlementReportResultsCsvWriter();

        var rows = new[]
        {
            _validRow with { GridArea = "999" },
        };

        // Act
        await sut.WriteAsync(memoryStream, rows, _testCulture);

        // Assert
        var text = await ReadStreamAsStringAsync(memoryStream);
        Assert.Equal("999", ReadValueForHeader(text, "METERINGGRIDAREAID"));
    }

    [Fact]
    public static async Task WriteAsync_GivenValidRow_WritesCorrectlyFormattedProcessType()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var sut = new SettlementReportResultsCsvWriter();

        var rows = new[]
        {
            _validRow with { CalculationType = CalculationType.BalanceFixing },
        };

        // Act
        await sut.WriteAsync(memoryStream, rows, _testCulture);

        // Assert
        var text = await ReadStreamAsStringAsync(memoryStream);
        Assert.Equal("D04", ReadValueForHeader(text, "ENERGYBUSINESSPROCESS"));
    }

    [Fact]
    public static async Task WriteAsync_GivenValidRow_WritesCorrectlyFormattedTime()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var sut = new SettlementReportResultsCsvWriter();

        var rows = new[]
        {
            _validRow with { Time = Instant.FromUtc(2021, 5, 6, 7, 8, 9) },
        };

        // Act
        await sut.WriteAsync(memoryStream, rows, _testCulture);

        // Assert
        var text = await ReadStreamAsStringAsync(memoryStream);
        Assert.Equal("2021-05-06T07:08:09Z", ReadValueForHeader(text, "STARTDATETIME"));
    }

    [Fact]
    public static async Task WriteAsync_GivenValidRow_WritesCorrectlyFormattedResolution()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var sut = new SettlementReportResultsCsvWriter();

        var rows = new[]
        {
            _validRow with { Resolution = "PT10M" },
        };

        // Act
        await sut.WriteAsync(memoryStream, rows, _testCulture);

        // Assert
        var text = await ReadStreamAsStringAsync(memoryStream);
        Assert.Equal("PT10M", ReadValueForHeader(text, "RESOLUTIONDURATION"));
    }

    [Theory]
    [InlineData(MeteringPointType.Consumption, "E17")]
    [InlineData(MeteringPointType.Production, "E18")]
    [InlineData(MeteringPointType.Exchange, "E20")]
    public static async Task WriteAsync_GivenValidRow_WritesCorrectlyFormattedMeteringPointType(MeteringPointType input, string expected)
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var sut = new SettlementReportResultsCsvWriter();

        var rows = new[]
        {
            _validRow with { MeteringPointType = input },
        };

        // Act
        await sut.WriteAsync(memoryStream, rows, _testCulture);

        // Assert
        var text = await ReadStreamAsStringAsync(memoryStream);
        Assert.Equal(expected, ReadValueForHeader(text, "TYPEOFMP"));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData(SettlementMethod.Flex, "D01")]
    [InlineData(SettlementMethod.NonProfiled, "E02")]
    public static async Task WriteAsync_GivenValidRow_WritesCorrectlyFormattedSettlementMethod(SettlementMethod? input, string expected)
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var sut = new SettlementReportResultsCsvWriter();

        var rows = new[]
        {
            _validRow with { SettlementMethod = input },
        };

        // Act
        await sut.WriteAsync(memoryStream, rows, _testCulture);

        // Assert
        var text = await ReadStreamAsStringAsync(memoryStream);
        Assert.Equal(expected, ReadValueForHeader(text, "SETTLEMENTMETHOD"));
    }

    [Theory]
    [InlineData(0, "0,000")]
    [InlineData(0.01, "0,010")]
    [InlineData(1.234, "1,234")]
    [InlineData(1000.234, "1000,234")]
    public static async Task WriteAsync_GivenValidRow_WritesCorrectlyFormattedQuantity(decimal input, string expected)
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var sut = new SettlementReportResultsCsvWriter();

        var rows = new[]
        {
            _validRow with { Quantity = input },
        };

        // Act
        await sut.WriteAsync(memoryStream, rows, _testCulture);

        // Assert
        var text = await ReadStreamAsStringAsync(memoryStream);
        Assert.Equal(expected, ReadValueForHeader(text, "ENERGYQUANTITY"));
    }

    private static async Task<string> ReadStreamAsStringAsync(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static string ReadValueForHeader(string csvText, string headerName)
    {
        var lines = csvText.Split(Environment.NewLine);

        var headerLine = lines[0];
        var headerValues = headerLine.Split(';');
        var headerIndex = Array.IndexOf(headerValues, headerName);

        var valueLine = lines[1];
        var valueValues = valueLine.Split(';');
        return valueValues[headerIndex];
    }
}
