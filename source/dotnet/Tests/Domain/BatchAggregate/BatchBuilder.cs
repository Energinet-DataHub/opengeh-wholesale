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

using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;

namespace Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;

public class BatchBuilder
{
    private BatchExecutionState? _state;
    private List<GridAreaCode> _gridAreaCodes = new() { new("805") };

    public BatchBuilder WithState(BatchExecutionState state)
    {
        _state = state;
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

    public Batch Build()
    {
        var batch = new Batch(ProcessType.BalanceFixing, _gridAreaCodes);
        if (_state != null)
            batch.SetPrivateProperty(b => b.ExecutionState, _state);
        return batch;
    }
}
