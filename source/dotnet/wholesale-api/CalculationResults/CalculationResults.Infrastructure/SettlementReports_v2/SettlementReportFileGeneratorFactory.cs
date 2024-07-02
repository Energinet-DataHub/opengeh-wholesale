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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportFileGeneratorFactory : ISettlementReportFileGeneratorFactory
{
    private readonly ISettlementReportEnergyResultRepository _settlementReportEnergyResultRepository;
    private readonly ISettlementReportWholesaleRepository _settlementReportWholesaleRepository;
    private readonly ISettlementReportChargeLinkPeriodsRepository _settlementReportChargeLinkPeriodsRepository;
    private readonly ISettlementReportMeteringPointMasterDataRepository _settlementReportMeteringPointMasterDataRepository;
    private readonly ISettlementReportMeteringPointTimeSeriesResultRepository _settlementReportMeteringPointTimeSeriesResultRepository;
    private readonly ISettlementReportMonthlyAmountRepository _settlementReportMonthlyAmountRepository;
    private readonly ISettlementReportChargePriceRepository _settlementChargePriceRepository;
    private readonly ISettlementReportMonthlyAmountTotalRepository _settlementReportMonthlyAmountTotalRepository;

    public SettlementReportFileGeneratorFactory(
        ISettlementReportEnergyResultRepository settlementReportEnergyResultRepository,
        ISettlementReportWholesaleRepository settlementReportWholesaleRepository,
        ISettlementReportChargeLinkPeriodsRepository settlementReportChargeLinkPeriodsRepository,
        ISettlementReportMeteringPointMasterDataRepository settlementReportMeteringPointMasterDataRepository,
        ISettlementReportMeteringPointTimeSeriesResultRepository settlementReportMeteringPointTimeSeriesResultRepository,
        ISettlementReportMonthlyAmountRepository settlementReportMonthlyAmountRepository,
        ISettlementReportChargePriceRepository settlementChargePriceRepository,
        ISettlementReportMonthlyAmountTotalRepository settlementReportMonthlyAmountTotalRepository)
    {
        _settlementReportEnergyResultRepository = settlementReportEnergyResultRepository;
        _settlementReportWholesaleRepository = settlementReportWholesaleRepository;
        _settlementReportChargeLinkPeriodsRepository = settlementReportChargeLinkPeriodsRepository;
        _settlementReportMeteringPointMasterDataRepository = settlementReportMeteringPointMasterDataRepository;
        _settlementReportMeteringPointTimeSeriesResultRepository = settlementReportMeteringPointTimeSeriesResultRepository;
        _settlementReportMonthlyAmountRepository = settlementReportMonthlyAmountRepository;
        _settlementChargePriceRepository = settlementChargePriceRepository;
        _settlementReportMonthlyAmountTotalRepository = settlementReportMonthlyAmountTotalRepository;
    }

    public ISettlementReportFileGenerator Create(SettlementReportFileContent fileContent)
    {
        switch (fileContent)
        {
            case SettlementReportFileContent.EnergyResult:
                return new EnergyResultFileGenerator(_settlementReportEnergyResultRepository);
            case SettlementReportFileContent.WholesaleResult:
                return new WholesaleResultFileGenerator(_settlementReportWholesaleRepository);
            case SettlementReportFileContent.ChargeLinksPeriods:
                return new ChargeLinkPeriodsFileGenerator(_settlementReportChargeLinkPeriodsRepository);
            case SettlementReportFileContent.MeteringPointMasterData:
                return new MeteringPointMasterDataFileGenerator(_settlementReportMeteringPointMasterDataRepository);
            case SettlementReportFileContent.Pt15M:
                return new MeteringPointTimeSeriesFileGenerator(_settlementReportMeteringPointTimeSeriesResultRepository, Resolution.Quarter);
            case SettlementReportFileContent.Pt1H:
                return new MeteringPointTimeSeriesFileGenerator(_settlementReportMeteringPointTimeSeriesResultRepository, Resolution.Hour);
            case SettlementReportFileContent.MonthlyAmount:
                return new MonthlyAmountFileGenerator(_settlementReportMonthlyAmountRepository);
            case SettlementReportFileContent.MonthlyAmountTotal:
                return new MonthlyAmountTotalFileGenerator(_settlementReportMonthlyAmountTotalRepository);
            case SettlementReportFileContent.ChargePrice:
                return new ChargePriceFileGenerator(_settlementChargePriceRepository);
            default:
                throw new ArgumentOutOfRangeException(nameof(fileContent), fileContent, null);
        }
    }
}
