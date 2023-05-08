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

using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep.Model;

namespace Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult;

/// <summary>
/// Result data from a specific step in a process
/// </summary>
/// <param name="Sum">Sum has a scale of 3</param>
/// <param name="Min">Min has a scale of 3</param>
/// <param name="Max">Max has a scale of 3</param>
/// <param name="PeriodStart"></param>
/// <param name="PeriodEnd"></param>
/// <param name="Resolution"></param>
/// <param name="Unit">kWh</param>
/// <param name="TimeSeriesPoints"></param>
/// <param name="ProcessType"></param>
/// <param name="TimeSeriesType"></param>
public sealed record ProcessStepResultDto(
    decimal Sum,
    decimal Min,
    decimal Max,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    string Resolution,
    string Unit,
    TimeSeriesPointDto[] TimeSeriesPoints,
    ProcessType ProcessType,
    TimeSeriesType TimeSeriesType);
