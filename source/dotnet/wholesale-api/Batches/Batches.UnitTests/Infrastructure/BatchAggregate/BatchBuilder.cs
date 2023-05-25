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

using Energinet.DataHub.Wholesale.Batches.Application.BatchAggregate;
using Energinet.DataHub.Wholesale.Batches.Application.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using NodaTime;
using Test.Core;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.BatchAggregate;

public class BatchBuilder
{
    private readonly Instant _periodStart;
    private readonly Instant _periodEnd;

    private BatchExecutionState? _state;
    private List<GridAreaCode> _gridAreaCodes = new() { new("805") };
    private ProcessType _processType = ProcessType.BalanceFixing;

    public BatchBuilder()
    {
        // Create a valid period representing January in a +01:00 offset (e.g. time zone "Europe/Copenhagen")
        // In order to be valid the last millisecond must be omitted
        var firstOfJanuary = DateTimeOffset.Parse("2021-01-31T23:00Z");
        _periodStart = Instant.FromDateTimeOffset(firstOfJanuary);
        _periodEnd = Instant.FromDateTimeOffset(firstOfJanuary.AddMonths(1));
    }

    public BatchBuilder WithStateSubmitted()
    {
        _state = BatchExecutionState.Submitted;
        return this;
    }

    public BatchBuilder WithStatePending()
    {
        _state = BatchExecutionState.Pending;
        return this;
    }

    public BatchBuilder WithStateExecuting()
    {
        _state = BatchExecutionState.Executing;
        return this;
    }

    public BatchBuilder WithStateCompleted()
    {
        _state = BatchExecutionState.Completed;
        return this;
    }

    public BatchBuilder WithGridAreaCode(string gridAreaCode)
    {
        _gridAreaCodes = new GridAreaCode(gridAreaCode).InList();
        return this;
    }

    public BatchBuilder WithGridAreaCodes(List<GridAreaCode> gridAreaCodes)
    {
        _gridAreaCodes = gridAreaCodes;
        return this;
    }

    public BatchBuilder WithProcessType(ProcessType processType)
    {
        _processType = processType;
        return this;
    }

    public Batch Build()
    {
        var batch = new Batch(
            _processType,
            _gridAreaCodes,
            _periodStart,
            _periodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
            Guid.NewGuid());
        var jobRunId = new CalculationId(new Random().Next(1, 1000));

        if (_state == BatchExecutionState.Submitted)
        {
            batch.MarkAsSubmitted(jobRunId);
        }
        else if (_state == BatchExecutionState.Pending)
        {
            batch.MarkAsSubmitted(jobRunId);
            batch.MarkAsPending();
        }
        else if (_state == BatchExecutionState.Executing)
        {
            batch.MarkAsSubmitted(jobRunId);
            batch.MarkAsPending();
            batch.MarkAsExecuting();
        }
        else if (_state == BatchExecutionState.Completed)
        {
            batch.MarkAsSubmitted(jobRunId);
            batch.MarkAsPending();
            batch.MarkAsExecuting();
            batch.MarkAsCompleted(SystemClock.Instance.GetCurrentInstant());
        }
        else if (_state != null)
        {
            throw new NotImplementedException();
        }

        return batch;
    }
}
