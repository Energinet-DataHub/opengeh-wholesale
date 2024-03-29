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

using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.GridArea;

public class GridAreaEntityConfiguration : IEntityTypeConfiguration<GridAreaOwner>
{
    public void Configure(EntityTypeBuilder<GridAreaOwner> builder)
    {
        builder.ToTable(nameof(GridAreaOwner));

        builder.HasKey(gao => gao.Id);
        builder.Property(gao => gao.Id)
            .ValueGeneratedNever();

        builder.Property(gao => gao.GridAreaCode);
        builder.Property(gao => gao.OwnerActorNumber);
        builder.Property(gao => gao.ValidFrom);
        builder.Property(gao => gao.SequenceNumber);
    }
}
