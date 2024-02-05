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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.WebApi.V3.Calculation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.V3.SettlementReport;

[ApiController]
[Route("v3/[controller]")]
public class SettlementReportController : V3ControllerBase
{
    private readonly ISettlementReportClient _settlementReportClient;

    public SettlementReportController(ISettlementReportClient settlementReportClient)
    {
        _settlementReportClient = settlementReportClient;
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

    /// <summary>
    /// Returns a stream containing the settlement report for calculation with <paramref name="calculationId" /> and <paramref name="gridAreaCode" />.
    /// </summary>
    /// <param name="calculationId">CalculationId</param>
    /// <param name="gridAreaCode">GridAreaCode</param>
    [HttpGet(Name = "GetSettlementReportAsStreamAsync")]
    [MapToApiVersion(Version)]
    [BinaryContent]
    [Authorize(Roles = Permissions.SettlementReportsManage)]
    public async Task GetAsync([Required] Guid calculationId, [Required] string gridAreaCode)
    {
        var outputStream = Response.BodyWriter.AsStream();

        await using (outputStream.ConfigureAwait(false))
        {
            await _settlementReportClient
                .GetSettlementReportAsync(calculationId, gridAreaCode, outputStream)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns a stream containing the settlement report for a calculation matching <paramref name="calculationId"/>
    /// </summary>
    /// <param name="calculationId">CalculationId</param>
    [HttpGet("ZippedBasisDataStream")]
    [MapToApiVersion(Version)]
    [BinaryContent]
    [Authorize(Roles = Permissions.SettlementReportsManage)]
    public async Task<IActionResult> GetSettlementReportAsync([Required] Guid calculationId)
    {
        var report = await _settlementReportClient.GetSettlementReportAsync(calculationId).ConfigureAwait(false);
        return Ok(report.Stream);
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
