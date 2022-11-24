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

namespace Energinet.DataHub.Wholesale.Domain.ProcessAggregate;

/// <summary>
/// Defines a wholesale process
/// </summary>
public class Process
{
    private readonly Guid _batchId;

    public Process(Guid batchId, string gridAreaCode)
    {
        _batchId = batchId;
        GridAreaCode = gridAreaCode;
        ProcessSteps = new List<ProcessStep>
        {
            new() { ProcessStepType = ProcessStepType.AggregateProductionPerGridArea },
        };
    }

    public Guid Id { get; }

    public ProcessType ProcessType => ProcessType.BalanceFixing;

    public IReadOnlyCollection<ProcessStep> ProcessSteps { get; }

    public string GridAreaCode { get; } = null!;

    /// <summary>
    /// Required by Entity Framework
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Process()
    {
        Id = Guid.NewGuid();
        ProcessSteps = new List<ProcessStep>();
    }
}
