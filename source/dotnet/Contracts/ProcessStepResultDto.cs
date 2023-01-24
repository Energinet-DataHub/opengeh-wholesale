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

namespace Energinet.DataHub.Wholesale.Contracts;

/// <summary>
/// Result data from a specific step in a process
/// </summary>
/// <param name="ProcessStepMeteringPointType"></param>
/// <param name="Sum">Sum should have a scale of 3</param>
/// <param name="Min">Min should have a scale of 3</param>
/// <param name="Max">Max should have a scale of 3</param>
/// <param name="TimeSeriesPoints"></param>
/// <param name="PeriodStart">In UTC time.</param>
/// <param name="PeriodEnd">In UTC time.</param>
public sealed record ProcessStepResultDto(
    ProcessStepMeteringPointType ProcessStepMeteringPointType,
    decimal Sum,
    decimal Min,
    decimal Max,
    TimeSeriesPointDto[] TimeSeriesPoints,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);
