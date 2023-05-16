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
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReport;
using NodaTime;
using ProcessType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient.ProcessType;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application;

public class SettlementReportApplicationService : ISettlementReportApplicationService
{
    private readonly IBatchApplicationService _batchRepository;
    private readonly ICalculationResultClient _calculationResultClient;
    private readonly ICsvWriter _csvWriter;
    private readonly ISettlementReportRepository _settlementReportRepository;

    public SettlementReportApplicationService(
        IBatchApplicationService batchRepository,
        ICalculationResultClient calculationResultClient,
        ICsvWriter csvWriter,
        ISettlementReportRepository settlementReportRepository)
    {
        _batchRepository = batchRepository;
        _calculationResultClient = calculationResultClient;
        _csvWriter = csvWriter;
        _settlementReportRepository = settlementReportRepository;
    }

    public async Task<SettlementReportDto> GetSettlementReportAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        var report = await _settlementReportRepository.GetSettlementReportAsync(Map(batch)).ConfigureAwait(false);
        return new SettlementReportDto(report.Stream);
    }

    public async Task WriteSettlementReportAsync(
        Stream destination,
        string[] gridAreaCodes,
        ProcessType processType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        string? energySupplier)
    {
        var calculationResults = await _calculationResultClient
            .GetSettlementReportResultAsync(
                gridAreaCodes,
                processType,
                Instant.FromDateTimeOffset(periodStart),
                Instant.FromDateTimeOffset(periodEnd),
                energySupplier)
            .ConfigureAwait(false);

        await _csvWriter
            .WriteRecordsAsync(destination, calculationResults)
            .ConfigureAwait(false);
    }

    public async Task GetSettlementReportAsync(Guid batchId, string gridAreaCode, Stream outputStream)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        await _settlementReportRepository
            .GetSettlementReportAsync(Map(batch), gridAreaCode, outputStream)
            .ConfigureAwait(false);
    }

    private BatchInfo Map(BatchDto batch)
    {
        return new BatchInfo
        {
            Id = batch.BatchId,
            PeriodStart = Instant.FromDateTimeOffset(batch.PeriodStart),
            PeriodEnd = Instant.FromDateTimeOffset(batch.PeriodEnd),
            GridAreaCodes = batch.GridAreaCodes.ToList(),
        };
    }
}
