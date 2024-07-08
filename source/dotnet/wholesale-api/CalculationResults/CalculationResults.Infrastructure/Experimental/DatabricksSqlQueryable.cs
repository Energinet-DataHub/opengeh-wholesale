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

using System.Collections;
using System.Linq.Expressions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;

// Wraps an existing IQueryable and adds our provider.
// All IQueryable methods are forwarded to the core IQueryable, which does the heavy lifting.
// Synchronous methods are not implemented; they are neither needed nor possible without using EF internal API.
public class DatabricksSqlQueryable : IOrderedQueryable
{
    private readonly IQueryable _queryCore;

    public DatabricksSqlQueryable(DatabricksQueryProvider queryProvider, IQueryable queryCore)
    {
        DatabricksQueryProvider = queryProvider;
        _queryCore = queryCore;
    }

    public Type ElementType => _queryCore.ElementType;

    public Expression Expression => _queryCore.Expression;

    public IQueryProvider Provider => DatabricksQueryProvider;

    protected DatabricksQueryProvider DatabricksQueryProvider { get; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotSupportedException("Synchronous and inline queries are not supported. Use async API instead.");
    }
}
