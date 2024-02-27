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

using System.IO.Compression;
using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReport.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Extensions;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReport.Fixtures;

public sealed class SettlementReportScenarioFixture : LazyFixtureBase
{
    public SettlementReportScenarioFixture(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
        Configuration = new WholesaleSubsystemConfiguration();
        ScenarioState = new SettlementReportScenarioState();
    }

    public SettlementReportScenarioState ScenarioState { get; }

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private WholesaleClient_V3 WholesaleClient { get; set; } = null!;

    private WholesaleSubsystemConfiguration Configuration { get; }

    public async Task<ZipArchive> StartDownloadingAsync(SettlementDownloadInput settlementDownloadInput)
    {
        using var fileResponse = await WholesaleClient.DownloadAsync(
            settlementDownloadInput.GridAreaCodes,
            settlementDownloadInput.CalculationType,
            settlementDownloadInput.CalculationPeriodStart,
            settlementDownloadInput.CalculationPeriodEnd);
        DiagnosticMessageSink.WriteDiagnosticMessage($"""
            Downloading settlement report for
            grid area codes {string.Join(", ", settlementDownloadInput.GridAreaCodes.ToArray())} and
            calculation type {settlementDownloadInput.CalculationType} started.
            """);

        return new ZipArchive(fileResponse.Stream, ZipArchiveMode.Read);
    }

    public async Task<string[]> SplitEntryIntoDataLinesAsync(ZipArchiveEntry entry)
    {
        using var stringReader = new StreamReader(entry.Open());
        var content = await stringReader.ReadToEndAsync();
        var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        return lines[1..];  //// The first line is the header.
    }

    public (int ConsumptionLines, int ProductionLines, int ExchangeLines) CountLinesPerTimeSeriesTypes(IEnumerable<string> lines)
    {
        var productionLines = 0;
        var exchangeLines = 0;
        var consumptionLines = 0;

        foreach (var line in lines)
        {
            var timeSeriesType = line.Split(",")[4];
            switch (timeSeriesType)
            {
                case "E17":
                    consumptionLines++;
                    break;
                case "E18":
                    productionLines++;
                    break;
                case "E20":
                    exchangeLines++;
                    break;
            }
        }

        return (ConsumptionLines: consumptionLines, ProductionLines: productionLines, ExchangeLines: exchangeLines);
    }

    public string GetUtcDate(string line)
    {
        return line.Split(",")[2];
    }

    protected override async Task OnInitializeAsync()
    {
        var isState = await DatabricksClientExtensions.StartWarehouseAndWaitForWarehouseStateAsync(Configuration.DatabricksWorkspace);
        DiagnosticMessageSink.WriteDiagnosticMessage($"Starting Databricks warehouse succeed: {isState}");
        if (!isState)
        {
            throw new Exception("Unable to start Databricks SQL warehouse. Reason unknown.");
        }

        WholesaleClient = await WholesaleClientFactory.CreateAsync(Configuration, useAuthentication: true);
    }

    protected override Task OnDisposeAsync()
    {
        return Task.CompletedTask;
    }
}
