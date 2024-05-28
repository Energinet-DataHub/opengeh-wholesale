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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Activities;

public sealed class ScatterSettlementReportFilesActivity
{
    private readonly ISettlementReportRequestHandler _settlementReportRequestHandler;

    public ScatterSettlementReportFilesActivity(ISettlementReportRequestHandler settlementReportRequestHandler)
    {
        _settlementReportRequestHandler = settlementReportRequestHandler;
    }

    [Function(nameof(ScatterSettlementReportFilesActivity))]
    public Task<IEnumerable<SettlementReportFileRequestDto>> Run([ActivityTrigger] ScatterSettlementReportFilesInput input)
    {
        return _settlementReportRequestHandler.RequestReportAsync(new SettlementReportRequestId(input.InstanceId), input.Request);
    }
}
