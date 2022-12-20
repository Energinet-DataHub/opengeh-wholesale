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

using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.JobRunner;

public class DatabricksCalculatorJobParametersFactory : ICalculatorJobParametersFactory
{
    private readonly IClock _clock;

    public DatabricksCalculatorJobParametersFactory(IClock clock)
    {
        _clock = clock;
    }

    public IEnumerable<string> CreateParameters(Batch batch)
    {
        var gridAreas = string.Join(", ", batch.GridAreaCodes.Select(c => c.Code));

        return new List<string>
        {
            $"--batch-id={batch.Id}",
            $"--batch-grid-areas=[{gridAreas}]",
            $"--batch-period-start-datetime={batch.PeriodStart}",
            $"--batch-period-end-datetime={batch.PeriodEnd}",
        };
    }
}
