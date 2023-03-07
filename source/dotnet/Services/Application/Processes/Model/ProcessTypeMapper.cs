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

using Energinet.DataHub.Wholesale.Contracts;

namespace Energinet.DataHub.Wholesale.Application.Processes.Model;

public class ProcessTypeMapper : IProcessTypeMapper
{
    public ProcessType MapFrom(Domain.ProcessAggregate.ProcessType processType) =>
        processType switch
        {
            Domain.ProcessAggregate.ProcessType.BalanceFixing => ProcessType.BalanceFixing,
            Domain.ProcessAggregate.ProcessType.Aggregation => ProcessType.Aggregation,
            _ => throw new NotImplementedException($"Cannot map process type '{processType}"),
        };

    public Domain.ProcessAggregate.ProcessType MapFrom(ProcessType processType) =>
        processType switch
        {
            ProcessType.BalanceFixing => Domain.ProcessAggregate.ProcessType.BalanceFixing,
            ProcessType.Aggregation => Domain.ProcessAggregate.ProcessType.Aggregation,
            _ => throw new NotImplementedException($"Cannot map process type '{processType}'"),
        };
}
