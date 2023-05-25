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

using Energinet.DataHub.Wholesale.Application.SettlementReport.Model;
using Energinet.DataHub.Wholesale.Common.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReport;

public interface ISettlementReportApplicationService
{
    Task<SettlementReportDto> GetSettlementReportAsync(Guid batchId);

    Task GetSettlementReportAsync(Guid batchId, string gridAreaCode, Stream outputStream);

    Task CreateCompressedSettlementReportAsync(
        Func<Stream> openDestinationStream,
        string[] gridAreaCodes,
        ProcessType processType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        string? energySupplier,
        string? csvLanguage);
}
