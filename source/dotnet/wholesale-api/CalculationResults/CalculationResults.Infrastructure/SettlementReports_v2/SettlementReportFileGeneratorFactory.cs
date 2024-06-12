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

using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportFileGeneratorFactory : ISettlementReportFileGeneratorFactory
{
    private readonly ISettlementReportEnergyResultRepository _settlementReportEnergyResultRepository;
    private readonly ISettlementReportWholesaleRepository _settlementReportWholesaleRepository;
    private readonly ISettlementReportChargeLinkPeriodsRepository _settlementReportChargeLinkPeriodsRepository;

    public SettlementReportFileGeneratorFactory(
        ISettlementReportEnergyResultRepository settlementReportEnergyResultRepository,
        ISettlementReportWholesaleRepository settlementReportWholesaleRepository,
        ISettlementReportChargeLinkPeriodsRepository settlementReportChargeLinkPeriodsRepository)
    {
        _settlementReportEnergyResultRepository = settlementReportEnergyResultRepository;
        _settlementReportWholesaleRepository = settlementReportWholesaleRepository;
        _settlementReportChargeLinkPeriodsRepository = settlementReportChargeLinkPeriodsRepository;
    }

    public ISettlementReportFileGenerator Create(SettlementReportFileContent fileContent)
    {
        switch (fileContent)
        {
            case SettlementReportFileContent.EnergyResultLatestPerDay:
                return new EnergyResultFileGenerator(_settlementReportEnergyResultRepository);
            case SettlementReportFileContent.EnergyResultForCalculationId:
                return new EnergyResultFileGenerator(_settlementReportEnergyResultRepository);
            case SettlementReportFileContent.WholesaleResult:
                return new WholesaleResultFileGenerator(_settlementReportWholesaleRepository, CalculationType.WholesaleFixing);
            case SettlementReportFileContent.FirstCorrectionResult:
                return new WholesaleResultFileGenerator(_settlementReportWholesaleRepository, CalculationType.FirstCorrectionSettlement);
            case SettlementReportFileContent.SecondCorrectionResult:
                return new WholesaleResultFileGenerator(_settlementReportWholesaleRepository, CalculationType.SecondCorrectionSettlement);
            case SettlementReportFileContent.ThirdCorrectionResult:
                return new WholesaleResultFileGenerator(_settlementReportWholesaleRepository, CalculationType.ThirdCorrectionSettlement);
            case SettlementReportFileContent.ChargeLinksPeriods:
                return new ChargeLinkPeriodsFileGenerator(_settlementReportChargeLinkPeriodsRepository);
            default:
                throw new ArgumentOutOfRangeException(nameof(fileContent), fileContent, null);
        }
    }
}
