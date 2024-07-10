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

using System.Data;
using System.Linq.Expressions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;

public abstract class DatabricksContextBase : IDisposable
{
    private readonly DatabricksSqlQueryExecutor _executor;
    private readonly DbContextCore _dbContext;

    protected DatabricksContextBase(DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor)
    {
        _dbContext = new DbContextCore(OnModelCreating);
        _executor = new DatabricksSqlQueryExecutor(_dbContext, databricksSqlWarehouseQueryExecutor);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContext.Dispose();
        }
    }

    protected abstract void OnModelCreating(ModelBuilder modelBuilder);

    protected IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        var dbSet = _dbContext.Set<TEntity>().AsQueryable();
        var provider = new DatabricksQueryProvider(_executor, (IAsyncQueryProvider)dbSet.Provider);
        return provider.CreateQuery<TEntity>(dbSet.Expression);
    }

    private sealed class DbContextCore : DbContext
    {
        private readonly Action<ModelBuilder> _createModel;

        public DbContextCore(Action<ModelBuilder> createModel)
        {
            _createModel = createModel;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.UseSqlServer(optBuilder =>
            {
                optBuilder.UseNodaTime();
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Emits custom function: DATE_TRUNC('day', FROM_UTC_TIMESTAMP(@param1, @param2))
            modelBuilder
                .HasDbFunction(typeof(DatabricksSqlQueryableExtensions.Functions).GetMethod(nameof(DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone))!)
                .HasTranslation(args =>
                {
                    var paramDate = args[0];
                    var paramTimeZone = args[1];

                    return new SqlFunctionExpression(
                        "DATE_TRUNC",
                        [
                            new SqlConstantExpression(Expression.Constant("DAY"), new StringTypeMapping("VARCHAR", DbType.String)),
                            new SqlFunctionExpression(
                                "FROM_UTC_TIMESTAMP",
                                [
                                    paramDate,
                                    paramTimeZone
                                ],
                                false,
                                [false, false],
                                paramDate.Type,
                                paramDate.TypeMapping)
                        ],
                        false,
                        [false, false],
                        paramDate.Type,
                        paramDate.TypeMapping);
                });

            // Emits custom function: TO_UTC_TIMESTAMP(@param1, @param2)
            modelBuilder
                .HasDbFunction(typeof(DatabricksSqlQueryableExtensions.Functions).GetMethod(nameof(DatabricksSqlQueryableExtensions.Functions.ToUtcFromTimeZoned))!)
                .HasTranslation(args =>
                {
                    var paramDate = args[0];
                    var paramTimeZone = args[1];

                    return new SqlFunctionExpression(
                        "TO_UTC_TIMESTAMP",
                        [
                            paramDate,
                            paramTimeZone
                        ],
                        false,
                        [false, false],
                        paramDate.Type,
                        paramDate.TypeMapping);
                });

            // Emits custom function: ARRAY_AGG(struct(@param1, @param2))
            // NOTE: Currently, EF Core does not support aggregations or generics in UDFs.
            // For now, we simply hard-code the struct combinations required (i.e. ValueTuple<Instant, decimal>).
            modelBuilder
                .HasDbFunction(typeof(DatabricksSqlQueryableExtensions.Functions).GetMethod(nameof(DatabricksSqlQueryableExtensions.Functions.AggregateArray))!)
                .HasTranslation(args =>
                {
                    var projections = args
                        .Cast<ScalarSubqueryExpression>()
                        .SelectMany(arg => arg.Subquery.Projection)
                        .Select(proj => proj.Expression);

                    return new SqlFunctionExpression(
                        "ARRAY_AGG",
                        [
                            new SqlFunctionExpression(
                                "struct",
                                projections,
                                false,
                                [false, false],
                                typeof(IEnumerable<(Instant Time, decimal Quantity)>),
                                new ByteArrayTypeMapping("varbinary")),
                        ],
                        false,
                        [false],
                        typeof(IEnumerable<(Instant Time, decimal Quantity)>),
                        new ByteArrayTypeMapping("varbinary"));
                })
                .Metadata.TypeMapping = new ByteArrayTypeMapping("varbinary");

            _createModel(modelBuilder);
        }
    }
}
