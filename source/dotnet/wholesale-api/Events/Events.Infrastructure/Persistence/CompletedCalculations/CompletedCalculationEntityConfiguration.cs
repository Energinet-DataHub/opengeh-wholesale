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
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;

public class CompletedCalculationEntityConfiguration : IEntityTypeConfiguration<CompletedCalculation>
{
    public void Configure(EntityTypeBuilder<CompletedCalculation> builder)
    {
        // TODO: Change to nameof(CompletedCalculation) when updating table name in database
        builder.ToTable("CompletedBatch");

        builder.HasKey(b => b.Id);
        builder
            .Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.PeriodStart);
        builder.Property(b => b.PeriodEnd);
        builder.Property(b => b.ProcessType);
        builder.Property(b => b.CompletedTime);
        builder.Property(b => b.PublishedTime);

        // Grid area codes are stored as a JSON array
        var gridAreaCodes = builder.Metadata
            .FindNavigation(nameof(CompletedCalculation.GridAreaCodes))!;
        gridAreaCodes.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder
            .Property(b => b.GridAreaCodes)
            .HasConversion(
                l => JsonSerializer.Serialize(l, (JsonSerializerOptions?)null),
                s => JsonSerializer.Deserialize<List<string>>(s, (JsonSerializerOptions?)null)!);
    }
}
