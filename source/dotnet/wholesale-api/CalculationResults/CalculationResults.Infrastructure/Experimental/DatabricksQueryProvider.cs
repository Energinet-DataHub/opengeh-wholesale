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
using Microsoft.EntityFrameworkCore.Query;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;

// The purpose of this provider is to ensure that all chained IQueryable are of our type DatabricksSqlQueryable.
// The actual implementations are always delegated to the core provider.
public sealed class DatabricksQueryProvider : IAsyncQueryProvider
{
    private readonly IAsyncQueryProvider _coreProvider;

    public DatabricksQueryProvider(DatabricksSqlQueryExecutor databricksSqlQueryExecutor, IAsyncQueryProvider coreProvider)
    {
        _coreProvider = coreProvider;
        DatabricksSqlQueryExecutor = databricksSqlQueryExecutor;
    }

    public DatabricksSqlQueryExecutor DatabricksSqlQueryExecutor { get; }

    public IQueryable CreateQuery(Expression expression)
    {
        return new DatabricksSqlQueryable(this, _coreProvider.CreateQuery(expression));
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new DatabricksSqlQueryable<TElement>(this, _coreProvider.CreateQuery<TElement>(expression));
    }

    public object? Execute(Expression expression)
    {
        if (!expression.Type.IsAssignableTo(typeof(IEnumerable)))
        {
            throw new NotSupportedException("Only queries that stream data are supported.");
        }

        return _coreProvider.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        if (!expression.Type.IsAssignableTo(typeof(IEnumerable)))
        {
            throw new NotSupportedException("Only queries that stream data are supported.");
        }

        return _coreProvider.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        if (!expression.Type.IsAssignableTo(typeof(IEnumerable)))
        {
            throw new NotSupportedException("Only queries that stream data are supported.");
        }

        return _coreProvider.ExecuteAsync<TResult>(expression, cancellationToken);
    }
}
