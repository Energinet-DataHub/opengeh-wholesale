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

using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Energinet.DataHub.Core.App.WebApp.Extensibility.Swashbuckle;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.WebApi.V3.Calculation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime.Extensions;
using CalculationState = Energinet.DataHub.Wholesale.Calculations.Interfaces.Models.CalculationState;

namespace Energinet.DataHub.Wholesale.WebApi.V3.SettlementReport;

[ApiController]
[Route("v3/[controller]")]
public class SettlementReportController : V3ControllerBase
{
    private readonly ISettlementReportClient _settlementReportClient;
    private readonly ICalculationsClient _calculationsClient;

    public SettlementReportController(
        ISettlementReportClient settlementReportClient,
        ICalculationsClient calculationsClient)
    {
        _settlementReportClient = settlementReportClient;
        _calculationsClient = calculationsClient;
    }

    /// <summary>
    /// Returns a subset of calculations that are valid for use with settlement reports.
    /// Settlement reports must access only a subset of data about calculations, as settlement reports are used by actors.
    /// </summary>
    [HttpGet(Name = "SearchCalculations")]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(List<SettlementReportCalculationDto>))]
    [Authorize(Roles = Permissions.SettlementReportsManage)]
    public async Task<IActionResult> SearchCalculationsAsync(
        [FromQuery] CalculationType calculationType,
        [FromQuery] string[] gridAreaCodes,
        [FromQuery] DateTimeOffset periodStart,
        [FromQuery] DateTimeOffset periodEnd)
    {
        // TODO: If user is part of Grid Access Provider, gridAreaCodes need to be checked for access.
        var calculations = await _calculationsClient
            .SearchAsync(
                gridAreaCodes,
                CalculationState.Completed,
                periodStart.ToInstant(),
                periodEnd.ToInstant(),
                CalculationTypeMapper.Map(calculationType))
            .ConfigureAwait(false);

        var calculationsForSettlementReports =
            from calculation in calculations
            from gridAreaCode in calculation.GridAreaCodes
            where gridAreaCodes.Contains(gridAreaCode)
            select new SettlementReportCalculationDto(
                calculation.CalculationId,
                calculation.PeriodStart,
                calculation.PeriodEnd,
                gridAreaCode);

        return Ok(calculationsForSettlementReports.ToList());
    }

    /// <summary>
    /// Downloads a compressed settlement report for the specified parameters.
    /// </summary>
    /// <param name="gridAreaCodes">A list of grid areas to create the settlement report for.</param>
    /// <param name="calculationType">Currently expects BalanceFixing only.</param>
    /// <param name="periodStart">The start date and time of the period covered by the settlement report.</param>
    /// <param name="periodEnd">The end date and time of the period covered by the settlement report.</param>
    /// <param name="energySupplier">Optional GLN/EIC identifier for an energy supplier.</param>
    /// <param name="csvFormatLocale">Optional locale used to format the CSV file, e.g. da-DK. Defaults to en-US.</param>
    [HttpGet("Download")]
    [MapToApiVersion(Version)]
    [BinaryContent]
    [Authorize(Roles = Permissions.SettlementReportsManage)]
    public Task DownloadSettlementReportAsync(
        [Required, FromQuery] string[] gridAreaCodes,
        [Required, FromQuery] CalculationType calculationType,
        [Required, FromQuery] DateTimeOffset periodStart,
        [Required, FromQuery] DateTimeOffset periodEnd,
        [FromQuery] string? energySupplier,
        [FromQuery] string? csvFormatLocale)
    {
        return _settlementReportClient
            .CreateCompressedSettlementReportAsync(
                () =>
                {
                    var settlementReportFileName = GetSettlementReportFileName(
                        gridAreaCodes,
                        calculationType,
                        periodStart,
                        periodEnd,
                        energySupplier);

                    Response.Headers.Append("Content-Type", "application/zip");
                    Response.Headers.Append("Content-Disposition", $"attachment; filename={settlementReportFileName}");

                    return Response.BodyWriter.AsStream();
                },
                gridAreaCodes,
                CalculationTypeMapper.Map(calculationType),
                periodStart,
                periodEnd,
                energySupplier,
                csvFormatLocale);
    }

    private static string GetSettlementReportFileName(
        string[] gridAreaCode,
        CalculationType calculationType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        string? energySupplier)
    {
        var energySupplierString = energySupplier is null ? string.Empty : $"_{energySupplier}";
        var gridAreaCodeString = string.Join("+", gridAreaCode);
        var calculationTypeString = calculationType switch
        {
            CalculationType.BalanceFixing => "D04",
            _ => string.Empty,
        };

        return $"Result_{gridAreaCodeString}{energySupplierString}_{periodStart:dd-MM-yyyy}_{periodEnd:dd-MM-yyyy}_{calculationTypeString}.zip";
    }
}
