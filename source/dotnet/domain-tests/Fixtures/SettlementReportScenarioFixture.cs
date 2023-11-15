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
using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Extensions;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    public sealed class SettlementReportScenarioFixture : LazyFixtureBase
    {
        public SettlementReportScenarioFixture(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            Configuration = new WholesaleDomainConfiguration();
            ScenarioState = new SettlementReportScenarioState();
        }

        public SettlementReportScenarioState ScenarioState { get; }

        /// <summary>
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        private WholesaleClient_V3 WholesaleClient { get; set; } = null!;

        private WholesaleDomainConfiguration Configuration { get; }

        public async Task<FileResponse> StartDownloadingAsync(SettlementDownloadInput settlementDownloadInput)
        {
            var fileResponse = await WholesaleClient.DownloadAsync(
                settlementDownloadInput.GridAreaCodes,
                settlementDownloadInput.ProcessType,
                settlementDownloadInput.CalculationPeriodStart,
                settlementDownloadInput.CalculationPeriodEnd);
            DiagnosticMessageSink.WriteDiagnosticMessage($"Downloading settlement report for " +
                                                         $"grid area codes {string.Join(", ", settlementDownloadInput.GridAreaCodes.ToArray())} and" +
                                                         $" process type {settlementDownloadInput.ProcessType} started.");
            return fileResponse;
        }

        public async Task<string[]> SplitEntryIntoLines(ZipArchiveEntry entry)
        {
            using var stringReader = new StreamReader(entry.Open());
            var content = await stringReader.ReadToEndAsync();
            var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            return lines;
        }

        protected override async Task OnInitializeAsync()
        {
            WholesaleClient = await WholesaleClientFactory.CreateWholesaleClientAsync(Configuration, useAuthentication: true);
        }

        protected override Task OnDisposeAsync()
        {
            return Task.CompletedTask;
        }
 }
}
