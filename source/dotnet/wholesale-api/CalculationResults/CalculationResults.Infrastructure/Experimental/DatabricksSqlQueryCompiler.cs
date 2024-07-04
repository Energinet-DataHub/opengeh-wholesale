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

using System.Text;
using System.Text.RegularExpressions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;

public sealed class DatabricksSqlQueryCompiler
{
    private readonly DbContext _context;

    public DatabricksSqlQueryCompiler(DbContext context)
    {
        _context = context;
    }

    public DatabricksStatement Compile(DatabricksSqlQueryable query)
    {
        using var dbCommand = query.CreateDbCommand();
        var sqlStatement = dbCommand.CommandText;
        var typeMapper = _context.GetService<IRelationalTypeMappingSource>();

        foreach (SqlParameter parameter in dbCommand.Parameters)
        {
            if (parameter.Value == null)
            {
                sqlStatement = sqlStatement.Replace(parameter.ParameterName, "NULL");
            }
            else
            {
                var mapped = typeMapper.GetMapping(parameter.Value.GetType()).GenerateSqlLiteral(parameter.Value);
                sqlStatement = sqlStatement.Replace(parameter.ParameterName, mapped);
            }
        }

        var databricksStatement = DatabricksStatement.FromRawSql(TranslateTransactToAnsi(sqlStatement));
        return databricksStatement.Build();
    }

    private static string TranslateTransactToAnsi(string transactSqlQuery)
    {
        var strBuilder = new StringBuilder(transactSqlQuery)
            .Replace('[', '`')
            .Replace(']', '`')
            .Replace('"', '\'')
            .Replace('"', '\'');

        var ansiSql = strBuilder.ToString();

        ansiSql = Regex.Replace(ansiSql, "N'([^']+)'", "'$1'");
        ansiSql = Regex.Replace(ansiSql, "OFFSET ([^\\s]+) ROWS", "OFFSET $1");
        ansiSql = Regex.Replace(ansiSql, "FETCH NEXT ([^\\s]+) ROWS ONLY", "LIMIT $1");
        ansiSql = Regex.Replace(ansiSql, "OFFSET ([^\\s]+) LIMIT ([^\\s]+)", "LIMIT $2 OFFSET $1");

        return ansiSql;
    }
}
