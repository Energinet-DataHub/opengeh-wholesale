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

using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using Energinet.DataHub.Wholesale.DomainTests.Features.SettlementReport.Fixtures;

namespace Energinet.DataHub.Wholesale.DomainTests.Features.SettlementReport.States;

public class SettlementReportScenarioState
{
    public SettlementDownloadInput SettlementDownloadInput { get; } = new();

    [NotNull]
    public FileResponse? SettlementReportFile { get; set; }

    [NotNull]
    public ZipArchive? CompressedSettlementReport { get; set; }

    [NotNull]
    public ZipArchiveEntry? Entry { get; set; }

    public string[] EntryDataLines { get; set; } = Array.Empty<string>();
}
