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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.ReceivedIntegrationEvent;

public class ReceivedIntegrationEventEntityConfiguration : IEntityTypeConfiguration<Application.IntegrationEvents.ReceivedIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<Application.IntegrationEvents.ReceivedIntegrationEvent> builder)
    {
        builder.ToTable(nameof(Application.IntegrationEvents.ReceivedIntegrationEvent));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.EventType);
        builder.Property(e => e.OccurredOn);
    }
}
