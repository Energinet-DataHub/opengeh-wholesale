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

using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

/// <summary>
/// Responsible for publishing integration events for a completed calculation, using Service Bus.
///
/// Copied from the Messaging package and refactored to allow us to send events immediately
/// and for a certain calculation only.
/// </summary>
public interface ICalculationIntegrationEventPublisher
{
    /// <summary>
    /// Publish integration events for a completed calculation, using Service Bus.
    /// </summary>
    Task PublishAsync(CalculationDto completedCalculation, string orchestrationInstanceId, CancellationToken cancellationToken);
}
