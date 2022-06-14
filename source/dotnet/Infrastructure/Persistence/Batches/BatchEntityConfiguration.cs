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
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;

public class BatchEntityConfiguration : IEntityTypeConfiguration<Batch>
{
    private static readonly string _aggregateTableName = nameof(Batch);

    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable(_aggregateTableName);

        builder.HasKey(b => b.Id);
        builder
            .Property(b => b.Id)
            .HasConversion(b => b.Id, id => new BatchId(id))
            .ValueGeneratedNever();

        builder.Property(b => b.ExecutionState);

        // Grid area IDs are stored as a JSON array
        builder
            .Property(b => b.GridAreaIds)
            .HasDefaultValue(new List<GridAreaId>())
            .HasConversion(
                l => JsonSerializer.Serialize(l.Select(id => id.Id), (JsonSerializerOptions?)null),
                s => JsonSerializer.Deserialize<List<Guid>>(s, (JsonSerializerOptions?)null)!.Select(id => new GridAreaId(id)).ToList());
    }
}
