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

using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.Databricks;

public sealed class SettlementReportMeteringPointMasterDataViewEntity
{
    [Column("calculation_id")]
    public Guid CalculationId { get; set; }

    [Column("calculation_type")]
    public string CalculationType { get; set; } = null!;

    [Column("grid_area_code")]
    public string GridAreaCode { get; set; } = null!;

    [Column("from_grid_area_code")]
    public string? GridAreaFromCode { get; set; }

    [Column("to_grid_area_code")]
    public string? GridAreaToCode { get; set; }

    [Column("from_date")]
    public Instant FromDate { get; set; }

    [Column("to_date")]
    public Instant? ToDate { get; set; }

    [Column("metering_point_id")]
    public string MeteringPointId { get; set; } = null!;

    [Column("metering_point_type")]
    public string MeteringPointType { get; set; } = null!;

    [Column("settlement_method")]
    public string? SettlementMethod { get; set; }

    [Column("energy_supplier_id")]
    public string? EnergySupplierId { get; set; }
}
