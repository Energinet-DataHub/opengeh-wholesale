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

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;

public class BatchEntityConfiguration : IEntityTypeConfiguration<Domain.BatchAggregate.Batch>
{
    private static readonly string _aggregateTableName = nameof(Domain.BatchAggregate.Batch);

    public void Configure(EntityTypeBuilder<Domain.BatchAggregate.Batch> builder)
    {
        builder.ToTable(_aggregateTableName);

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.ExecutionState);

        // Grid area IDs are stored as a JSON array
        builder
            .Property(b => b.GridAreaIds)
            .HasConversion(
                l => JsonSerializer.Serialize(l, (JsonSerializerOptions?)null),
                s => JsonSerializer.Deserialize<List<Guid>>(s, (JsonSerializerOptions?)null) ?? new List<Guid>());
    }
}
