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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

public class DatabricksSqlResponse
{
    private readonly DatabricksSqlResponseState _state;
    private readonly Table? _table;

    private DatabricksSqlResponse(DatabricksSqlResponseState state, Table? table)
    {
        _state = state;
        _table = table;
    }

    public static DatabricksSqlResponse CreateAsCancelled()
    {
        return new DatabricksSqlResponse(DatabricksSqlResponseState.Cancelled, null);
    }

    public static DatabricksSqlResponse CreateAsSucceeded(Table resultTable)
    {
        return new DatabricksSqlResponse(DatabricksSqlResponseState.Succeeded, resultTable);
    }

    public static DatabricksSqlResponse CreateAsFailed()
    {
        return new DatabricksSqlResponse(DatabricksSqlResponseState.Failed, null);
    }

    public static DatabricksSqlResponse CreateAsPending()
    {
        return new DatabricksSqlResponse(DatabricksSqlResponseState.Pending, null);
    }

    public DatabricksSqlResponseState State => _state;

    public Table? Table => _table;
}
