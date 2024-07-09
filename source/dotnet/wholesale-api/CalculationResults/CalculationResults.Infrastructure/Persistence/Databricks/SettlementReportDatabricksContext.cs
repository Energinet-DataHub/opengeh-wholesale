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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.Databricks;

public sealed class SettlementReportDatabricksContext : DatabricksContextBase, ISettlementReportDatabricksContext
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;

    public SettlementReportDatabricksContext(
        IOptions<DeltaTableOptions> deltaTableOptions,
        DatabricksSqlWarehouseQueryExecutor sqlWarehouseQueryExecutor)
        : base(sqlWarehouseQueryExecutor)
    {
        _deltaTableOptions = deltaTableOptions;
    }

    public IQueryable<SettlementReportWholesaleViewEntity> WholesaleView => Set<SettlementReportWholesaleViewEntity>();

    public IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> EnergyResultPointsPerGridAreaView => Set<SettlementReportEnergyResultPointsPerGridAreaViewEntity>();

    public IQueryable<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity> EnergyResultPointsPerEnergySupplierGridAreaView => Set<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_deltaTableOptions.Value.SettlementReportSchemaName);
        modelBuilder.ApplyConfiguration(new SettlementReportWholesaleViewEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SettlementReportEnergyResultPointsPerGridAreaViewEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntityConfiguration());
    }
}
