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

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.States;

public class ConcurrentRunsScenarioState
{
    /// <summary>
    /// Use to specify the parameters to use for each job we start.
    /// Any occurrence of 'Guid' will be replaced with a new Guid.
    /// </summary>
    [NotNull]
    public IReadOnlyCollection<string>? JobParametersTemplate { get; set; }

    /// <summary>
    /// A list of job runs (started jobs).
    /// </summary>
    [NotNull]
    public IReadOnlyDictionary<long, SettlementReportJobState>? JobRuns { get; set; }

    /// <summary>
    /// A list of job runs (started jobs) above the <see cref="ExpectedMaxConcurrentRuns"/>.
    /// </summary>
    [NotNull]
    public IReadOnlyDictionary<long, SettlementReportJobState>? ExceedingJobRuns { get; set; }

    /// <summary>
    /// The expected max. concurrent runs possible.
    /// The test will try to start one more to verify that it is queued.
    /// </summary>
    public int ExpectedMaxConcurrentRuns { get; set; }
}
