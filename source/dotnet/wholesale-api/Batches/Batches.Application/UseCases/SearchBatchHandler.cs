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

using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Application.UseCases;

public class SearchBatchHandler : ISearchBatchHandler
{
    private readonly IBatchRepository _batchRepository;
    private readonly IBatchDtoMapper _batchDtoMapper;

    public SearchBatchHandler(IBatchRepository batchRepository, IBatchDtoMapper batchDtoMapper)
    {
        _batchRepository = batchRepository;
        _batchDtoMapper = batchDtoMapper;
    }

    public async Task<IEnumerable<BatchDto>> SearchAsync(
        IEnumerable<string> filterByGridAreaCodes,
        BatchState? filterByExecutionState,
        DateTimeOffset? minExecutionTime,
        DateTimeOffset? maxExecutionTime,
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd)
    {
        var executionStateFilter = filterByExecutionState switch
        {
            null => Array.Empty<BatchExecutionState>(),
            BatchState.Pending => new[] { BatchExecutionState.Created, BatchExecutionState.Submitted, BatchExecutionState.Pending },
            BatchState.Executing => new[] { BatchExecutionState.Executing },
            BatchState.Completed => new[] { BatchExecutionState.Completed },
            BatchState.Failed => new[] { BatchExecutionState.Failed },
            _ => throw new ArgumentOutOfRangeException(nameof(filterByExecutionState)),
        };

        var gridAreaFilter = filterByGridAreaCodes
            .Select(g => new GridAreaCode(g))
            .ToList();

        var minExecutionTimeStart = ConvertToInstant(minExecutionTime);
        var maxExecutionTimeStart = ConvertToInstant(maxExecutionTime);
        var periodStartInstant = ConvertToInstant(periodStart);
        var periodEndInstant = ConvertToInstant(periodEnd);

        var batches = await _batchRepository
            .SearchAsync(
                gridAreaFilter,
                executionStateFilter,
                minExecutionTimeStart,
                maxExecutionTimeStart,
                periodStartInstant,
                periodEndInstant)
            .ConfigureAwait(false);

        return batches.Select(_batchDtoMapper.Map);
    }

    private static Instant? ConvertToInstant(DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset == null
            ? null
            : Instant.FromDateTimeOffset(dateTimeOffset.Value);
    }
}
