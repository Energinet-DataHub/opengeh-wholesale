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
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Calculations;

public class CalculationEntityConfiguration : IEntityTypeConfiguration<Calculation>
{
    public void Configure(EntityTypeBuilder<Calculation> builder)
    {
        builder.ToTable(nameof(Calculation));

        builder.HasKey(b => b.Id);
        builder
            .Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.ExecutionState);
        builder.Property(b => b.PeriodStart);
        builder.Property(b => b.PeriodEnd);
        builder.Property(b => b.ExecutionTimeStart);
        builder.Property(b => b.ExecutionTimeEnd);
        builder.Property(b => b.AreSettlementReportsCreated);
        builder.Property(b => b.CalculationId).HasConversion(
            calculationId => calculationId == null ? (long?)null : calculationId.Id,
            calculationId => calculationId == null ? null : new CalculationId(calculationId.Value));
        builder.Property(b => b.ProcessType);
        builder.Property(b => b.CreatedTime);
        builder.Property(b => b.CreatedByUserId);

        // Grid area IDs are stored as a JSON array
        var gridAreaCodes = builder.Metadata
            .FindNavigation(nameof(Calculation.GridAreaCodes))!;
        gridAreaCodes.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder
            .Property(b => b.GridAreaCodes)
            .HasConversion(
                l => JsonSerializer.Serialize(l.Select(code => code.Code), (JsonSerializerOptions?)null),
                s => JsonSerializer.Deserialize<List<string>>(s, (JsonSerializerOptions?)null)!.Select(code => new GridAreaCode(code)).ToList());
    }
}
