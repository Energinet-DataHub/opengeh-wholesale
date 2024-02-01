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

using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;
using Test.Core;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationAggregate;

public class CalculationBuilder
{
    public static readonly DateTimeOffset FirstOfJanuary2022 = DateTimeOffset.Parse("2021-12-31T23:00Z");

    private Instant _periodStart;
    private Instant _periodEnd;

    private CalculationExecutionState? _state;
    private List<GridAreaCode> _gridAreaCodes = new() { new("805") };
    private CalculationType _calculationType = CalculationType.BalanceFixing;

    public CalculationBuilder()
    {
        // Create a valid period representing January in a +01:00 offset (e.g. time zone "Europe/Copenhagen")
        // In order to be valid the last millisecond must be omitted
        _periodStart = Instant.FromDateTimeOffset(FirstOfJanuary2022);
        _periodEnd = Instant.FromDateTimeOffset(FirstOfJanuary2022.AddMonths(1));
    }

    public CalculationBuilder WithStateSubmitted()
    {
        _state = CalculationExecutionState.Submitted;
        return this;
    }

    public CalculationBuilder WithStatePending()
    {
        _state = CalculationExecutionState.Pending;
        return this;
    }

    public CalculationBuilder WithStateExecuting()
    {
        _state = CalculationExecutionState.Executing;
        return this;
    }

    public CalculationBuilder WithStateCompleted()
    {
        _state = CalculationExecutionState.Completed;
        return this;
    }

    public CalculationBuilder WithGridAreaCode(string gridAreaCode)
    {
        _gridAreaCodes = new GridAreaCode(gridAreaCode).InList();
        return this;
    }

    public CalculationBuilder WithGridAreaCodes(List<GridAreaCode> gridAreaCodes)
    {
        _gridAreaCodes = gridAreaCodes;
        return this;
    }

    public CalculationBuilder WithCalculationType(CalculationType calculationType)
    {
        _calculationType = calculationType;
        return this;
    }

    public CalculationBuilder WithPeriodStart(Instant periodStart)
    {
        _periodStart = periodStart;
        return this;
    }

    public CalculationBuilder WithPeriodEnd(Instant periodEnd)
    {
        _periodEnd = periodEnd;
        return this;
    }

    public Calculation Build()
    {
        var batch = new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            _calculationType,
            _gridAreaCodes,
            _periodStart,
            _periodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
            Guid.NewGuid(),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks);
        var jobRunId = new CalculationJobId(new Random().Next(1, 1000));

        if (_state == CalculationExecutionState.Submitted)
        {
            batch.MarkAsSubmitted(jobRunId);
        }
        else if (_state == CalculationExecutionState.Pending)
        {
            batch.MarkAsSubmitted(jobRunId);
            batch.MarkAsPending();
        }
        else if (_state == CalculationExecutionState.Executing)
        {
            batch.MarkAsSubmitted(jobRunId);
            batch.MarkAsPending();
            batch.MarkAsExecuting();
        }
        else if (_state == CalculationExecutionState.Completed)
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
