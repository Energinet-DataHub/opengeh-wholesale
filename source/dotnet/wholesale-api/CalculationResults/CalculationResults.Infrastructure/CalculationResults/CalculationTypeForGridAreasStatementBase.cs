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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

// TODO (MWO): Rename class and arguments
public abstract class CalculationTypeForGridAreasStatementBase(
    string gridAreaCodeColumnName,
    string calculationTypeColumnName) : DatabricksStatement
{
    // TODO (MWO): Rename fields
    private readonly string _gridAreaCodeColumnName = gridAreaCodeColumnName;
    private readonly string _calculationTypeColumnName = calculationTypeColumnName;

    protected abstract string GetSource();

    protected abstract string GetSelection();

    protected override string GetSqlStatement()
    {
        return $"""
                SELECT {_gridAreaCodeColumnName}, {_calculationTypeColumnName}
                FROM {GetSource()} wrv
                WHERE {GetSelection()}
                GROUP BY {_gridAreaCodeColumnName}, {_calculationTypeColumnName}
                """;
    }
}
