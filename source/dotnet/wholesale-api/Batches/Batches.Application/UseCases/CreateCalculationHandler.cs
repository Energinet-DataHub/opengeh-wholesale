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

using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Application.UseCases;

public class CreateCalculationHandler : ICreateCalculationHandler
{
    private readonly ICalculationFactory _calculationFactory;
    private readonly ICalculationRepository _calculationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateCalculationHandler(
        ICalculationFactory calculationFactory,
        ICalculationRepository calculationRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateCalculationHandler> logger)
    {
        _calculationFactory = calculationFactory;
        _calculationRepository = calculationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(CreateCalculationCommand command)
    {
        var query = $@"INSERT INTO [batches].[Batch] 
                   ([Id], [GridAreaCodes], [ExecutionState], [PeriodStart], [PeriodEnd], [ExecutionTimeStart], [AreSettlementReportsCreated], [ProcessType], [CreatedByUserId], [CreatedTime], [Version]) 
                   SELECT * FROM (VALUES 
                       (@Id, @GridAreaCodes, @ExecutionState, @PeriodStart, @PeriodEnd, @ExecutionTimeStart, @AreSettlementReportsCreated, @ProcessType, @CreatedByUserId, @CreatedTime, @Version)
                   ) AS s([Id], [GridAreaCodes], [ExecutionState], [PeriodStart], [PeriodEnd], [ExecutionTimeStart], [AreSettlementReportsCreated], [ProcessType], [CreatedByUserId], [CreatedTime], [Version]) 
                   WHERE NOT EXISTS (
                       SELECT * FROM [batches].[Batch] t WITH (UPDLOCK) 
                       WHERE s.[ProcessType] = t.[ProcessType] AND ([ExecutionState] != 2 AND [ExecutionState] != 3 AND [ExecutionState] != 4)
                   )";

        var calculation = _calculationFactory.Create(command.ProcessType, command.GridAreaCodes, command.StartDate, command.EndDate, command.CreatedByUserId);
        var id = new SqlParameter("Id", calculation.Id);
        var gridAreaCodes = new SqlParameter("GridAreaCodes", "[" + string.Join(", ", calculation.GridAreaCodes.Select(code => $"\"{code.Code}\"")) + "]");
        var executionState = new SqlParameter("ExecutionState", (int)calculation.ExecutionState);
        var periodStart =
            new SqlParameter("PeriodStart", SqlDbType.DateTime2) { Value = calculation.PeriodStart.ToDateTimeUtc() };
        var periodEnd =
            new SqlParameter("PeriodEnd", SqlDbType.DateTime2) { Value = calculation.PeriodEnd.ToDateTimeUtc() };
        var executionTimeStart =
            new SqlParameter("ExecutionTimeStart", SqlDbType.DateTime2)
            {
                Value = calculation.ExecutionTimeStart?.ToDateTimeUtc(),
            };
        var areSettlementReportsCreated =
            new SqlParameter("AreSettlementReportsCreated", calculation.AreSettlementReportsCreated);
        var processType = new SqlParameter("ProcessType", (int)calculation.ProcessType);
        var createdByUserId = new SqlParameter("CreatedByUserId", calculation.CreatedByUserId);
        var createdTime = new SqlParameter("CreatedTime", SqlDbType.DateTime2) { Value = calculation.CreatedTime.ToDateTimeUtc() };
        var version = new SqlParameter("Version", calculation.Version);

        var formattableString = FormattableStringFactory.Create(query, id, gridAreaCodes, executionState, periodStart, periodEnd, executionTimeStart, areSettlementReportsCreated, processType, createdByUserId, createdTime, version);
        var af = await _calculationRepository.ExecuteSqlAsync(formattableString).ConfigureAwait(false);

        if (af == 0)
        {
            throw new BusinessValidationException(
                $"There is already a {Enum.GetName(typeof(ProcessType), command.ProcessType)} is executing.");
        }

        _logger.LogInformation("Calculation created with id {calculation_id}", calculation.Id);
        return calculation.Id;
    }
}
