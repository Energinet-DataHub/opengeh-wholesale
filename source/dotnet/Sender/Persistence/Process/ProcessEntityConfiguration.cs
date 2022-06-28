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

using Energinet.DataHub.Wholesale.Sender.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Wholesale.Sender.Persistence.Process;

public class ProcessEntityConfiguration : IEntityTypeConfiguration<Domain.Process>
{
    public void Configure(EntityTypeBuilder<Domain.Process> builder)
    {
        builder.ToTable(nameof(Domain.Process));
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder
            .Property(p => p.MessageHubReference)
            .HasConversion(
                reference => reference.Value,
                value => new MessageHubReference(value));
        builder.Property(p => p.GridAreaCode);
    }
}
