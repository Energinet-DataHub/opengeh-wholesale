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

using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.Databricks;

public sealed class SettlementReportMonthlyAmountsViewEntity
{
    [Column("calculation_id")]
    public Guid CalculationId { get; set; }

    [Column("calculation_type")]
    public string CalculationType { get; set; } = null!;

    [Column("result_id")]
    public string ResultId { get; set; } = null!;

    [Column("grid_area_code")]
    public string GridAreaCode { get; set; } = null!;

    [Column("energy_supplier_id")]
    public string EnergySupplierId { get; set; } = null!;

    [Column("time")]
    public Instant Time { get; set; }

    [Column("quantity_unit")]
    public string QuantityUnit { get; set; } = null!;

    [Column("amount", TypeName = "decimal(18,6)")]
    public decimal? Amount { get; set; }

    [Column("charge_type")]
    public string? ChargeType { get; set; }

    [Column("charge_code")]
    public string? ChargeCode { get; set; }

    [Column("charge_owner_id")]
    public string? ChargeOwnerId { get; set; }

    [Column("is_tax", TypeName = "int")]
    public bool? IsTax { get; set; }
}
