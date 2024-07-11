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

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.Databricks;

public sealed class SettlementReportChargePriceViewEntityConfiguration : IEntityTypeConfiguration<SettlementReportChargePriceResultViewEntity>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public void Configure(EntityTypeBuilder<SettlementReportChargePriceResultViewEntity> builder)
    {
        builder.ToTable("charge_prices_v1");
        builder.HasNoKey();
        builder.Property(x => x.PricePoints)
            .HasConversion(
                new ValueConverter<SettlementReportChargePriceResultViewPricePointEntity[], string>(
                    v => JsonSerializer.Serialize(v, _jsonSerializerOptions),
                    v => JsonSerializer.Deserialize<SettlementReportChargePriceResultViewPricePointEntity[]>(v, _jsonSerializerOptions)!));
    }
}
