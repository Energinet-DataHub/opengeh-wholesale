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

using System.Data.SqlClient;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Database;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;

/// <summary>
/// A database manager for managing a database to be used by the Durable Task SQL Provider.
/// We should not create the schema, only an empty database with the expected collation name.
/// Durable Function will create the schema when the functions host starts up.
/// See https://microsoft.github.io/durabletask-mssql/#/architecture?id=table-schema
/// </summary>
public class DurableTaskDatabaseManager : SqlServerDatabaseManager<DbContext>
{
    // TODO:
    // We could move this class to TestCommon. See also "DurableTaskManager".
    public DurableTaskDatabaseManager()
        : base("WholesaleDT", DurableTaskCollationName)
    {
    }

    /// <summary>
    /// We don't have a specific DbContext as we don't need a schema.
    /// </summary>
    public override DbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>()
            .UseSqlServer(ConnectionString);

        return (DbContext)Activator.CreateInstance(typeof(DbContext), optionsBuilder.Options)!;
    }

    /// <summary>
    /// Disable multi-tenancy to be able to configure Task Hub Name.
    /// See https://microsoft.github.io/durabletask-mssql/#/multitenancy?id=enabling-shared-schema-multitenancy
    /// </summary>
    public void DisableMultiTenancy()
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand("EXECUTE dt.SetGlobalSetting @Name='TaskHubMode', @Value=0", connection);
        connection.Open();
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Do not create any schema.
    /// </summary>
    protected override Task<bool> CreateDatabaseSchemaAsync(DbContext context)
    {
        return Task.FromResult(CreateDatabaseSchema(context));
    }

    /// <summary>
    /// Do not create any schema.
    /// </summary>
    protected override bool CreateDatabaseSchema(DbContext context)
    {
        return true;
    }
}
