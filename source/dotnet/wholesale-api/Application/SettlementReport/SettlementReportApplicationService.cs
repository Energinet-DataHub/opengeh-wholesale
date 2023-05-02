﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReport;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.SettlementReport;

public class SettlementReportApplicationService : ISettlementReportApplicationService
{
    private readonly IBatchRepository _batchRepository;
    private readonly ISettlementReportRepository _settlementReportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SettlementReportApplicationService(
        IBatchRepository batchRepository,
        ISettlementReportRepository settlementReportRepository,
        IUnitOfWork unitOfWork)
    {
        _batchRepository = batchRepository;
        _settlementReportRepository = settlementReportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateSettlementReportAsync(BatchCompletedEventDto batchCompletedEvent)
    {
        var batch = await _batchRepository.GetAsync(batchCompletedEvent.BatchId).ConfigureAwait(false);
        await _settlementReportRepository.CreateSettlementReportsAsync(Map(batch)).ConfigureAwait(false);
        batch.AreSettlementReportsCreated = true;
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task<SettlementReportDto> GetSettlementReportAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        var report = await _settlementReportRepository.GetSettlementReportAsync(Map(batch)).ConfigureAwait(false);
        return new SettlementReportDto(report.Stream);
    }

    public async Task GetSettlementReportAsync(Guid batchId, string gridAreaCode, Stream outputStream)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        await _settlementReportRepository
            .GetSettlementReportAsync(Map(batch), gridAreaCode, outputStream)
            .ConfigureAwait(false);
    }

    private BatchInfo Map(Batch batch)
    {
        return new BatchInfo
        {
            Id = batch.Id,
            PeriodStart = batch.PeriodStart,
            PeriodEnd = batch.PeriodEnd,
            GridAreaCodes = batch.GridAreaCodes.Select(c => c.Code).ToList(),
        };
    }
}
