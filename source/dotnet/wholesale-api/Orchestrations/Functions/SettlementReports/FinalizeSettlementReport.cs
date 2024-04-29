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

using Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports;

public class FinalizeSettlementReport
{
    private readonly ILogger _logger;

    public FinalizeSettlementReport(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<FinalizeSettlementReport>();
    }

    [Function(nameof(FinalizeSettlementReport))]
    public Task Run([ActivityTrigger] ZippedSettlementReportResult input)
    {
        // clean up
        // remove temporary files
        return Task.CompletedTask;
    }
}
