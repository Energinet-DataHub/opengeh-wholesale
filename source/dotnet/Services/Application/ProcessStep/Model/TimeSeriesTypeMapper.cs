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

using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;

namespace Energinet.DataHub.Wholesale.Application.ProcessStep.Model;

public static class TimeSeriesTypeMapper
{
    public static TimeSeriesType Map(Contracts.TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            Contracts.TimeSeriesType.NonProfiledConsumption => TimeSeriesType.NonProfiledConsumption,
            Contracts.TimeSeriesType.FlexConsumption => TimeSeriesType.FlexConsumption,
            Contracts.TimeSeriesType.Production => TimeSeriesType.Production,
            _ => throw new ArgumentOutOfRangeException(nameof(timeSeriesType), timeSeriesType, null),
        };
    }

    public static Contracts.TimeSeriesType Map(TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            TimeSeriesType.NonProfiledConsumption => Contracts.TimeSeriesType.NonProfiledConsumption,
            TimeSeriesType.FlexConsumption => Contracts.TimeSeriesType.FlexConsumption,
            TimeSeriesType.Production => Contracts.TimeSeriesType.Production,
            _ => throw new ArgumentOutOfRangeException(nameof(timeSeriesType), timeSeriesType, null),
        };
    }
}
