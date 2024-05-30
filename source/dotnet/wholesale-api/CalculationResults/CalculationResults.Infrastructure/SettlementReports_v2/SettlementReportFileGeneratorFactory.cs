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

using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportFileGeneratorFactory : ISettlementReportFileGeneratorFactory
{
    private readonly ISettlementReportDataRepository _dataRepository;

    public SettlementReportFileGeneratorFactory(ISettlementReportDataRepository dataRepository)
    {
        _dataRepository = dataRepository;
    }

    public ISettlementReportFileGenerator Create(SettlementReportFileContent fileContent)
    {
        switch (fileContent)
        {
            case SettlementReportFileContent.BalanceFixingResult:
                return new EnergyResultFileGenerator(_dataRepository);
            default:
                throw new ArgumentOutOfRangeException(nameof(fileContent), fileContent, null);
        }
    }
}
