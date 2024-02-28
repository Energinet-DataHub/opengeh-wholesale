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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;

public interface ISettlementReportClient
{
    Task<SettlementReportDto> GetSettlementReportAsync(Guid calculationId);

    Task GetSettlementReportAsync(Guid calculationId, string gridAreaCode, Stream outputStream);

    Task CreateCompressedSettlementReportAsync(
        Func<Stream> openDestinationStream,
        string[] gridAreaCodes,
        CalculationType calculationType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        string? energySupplier,
        string? csvFormatLocale);
}
