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

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Private setters are needed by EF Core")]
public class DatabaseContext : DbContext, IDatabaseContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    // Added to support Moq in tests
    public DatabaseContext()
    {
    }

    public virtual DbSet<Batch> Batches { get; private set; } = null!;

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BatchEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
