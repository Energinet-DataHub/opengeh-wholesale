# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from package.codelists import MeteringPointType
from package.steps.aggregation.transformations import (
    create_dataframe_from_aggregation_result_schema,
)
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when, lit
from package.constants import ResultKeyName
from package.constants import Colname

sys_cor_energy_supplier = "SysCor_EnergySupplier"
sys_cor_grid_area = "SysCor_GridArea"
adjusted_sum_quantity = "adjusted_sum_quantity"


# step 11
def adjust_production(
    hourly_production_result_df: DataFrame,
    negative_grid_loss_result_df: DataFrame,
    grid_loss_responsible_df: DataFrame
) -> DataFrame:
    # select columns from dataframe that contains information about metering points registered as SystemCorrection to use in join.
    sc_df = grid_loss_responsible_df.selectExpr(
        Colname.from_date,
        Colname.to_date,
        f"{Colname.energy_supplier_id} as {sys_cor_energy_supplier}",
        f"{Colname.grid_area} as {sys_cor_grid_area}",
        Colname.is_negative_grid_loss_responsible,
    )

    # join result dataframes from previous steps on time window and grid area.
    df = hourly_production_result_df.join(
        negative_grid_loss_result_df,
        [Colname.time_window, Colname.grid_area],
        "inner",
    ).select(
        hourly_production_result_df[Colname.grid_area],
        hourly_production_result_df[Colname.to_grid_area],
        hourly_production_result_df[Colname.from_grid_area],
        hourly_production_result_df[Colname.balance_responsible_id],
        hourly_production_result_df[Colname.energy_supplier_id],
        hourly_production_result_df[Colname.time_window],
        hourly_production_result_df[Colname.sum_quantity],
        hourly_production_result_df[Colname.quality],
        hourly_production_result_df[Colname.metering_point_type],
        hourly_production_result_df[Colname.settlement_method],
        negative_grid_loss_result_df[Colname.positive_grid_loss],
        negative_grid_loss_result_df[Colname.negative_grid_loss],
    )

    # join information from system correction dataframe on to joined result dataframe with information about which energy supplier,
    # that is responsible for system correction in the given time window from the joined result dataframe.
    df = df.join(
        sc_df,
        when(
            col(Colname.to_date).isNotNull(),
            col(Colname.time_window_start) <= col(Colname.to_date),
        ).otherwise(True)
        & (col(Colname.time_window_start) >= col(Colname.from_date))
        & (
            col(Colname.to_date).isNull()
            | (col(Colname.time_window_end) <= col(Colname.to_date))
        )
        & (col(Colname.grid_area) == col(sys_cor_grid_area))
        & (col(Colname.is_negative_grid_loss_responsible)),
        "left",
    )

    # update function that selects the sum of two columns if condition is met, or selects data from a single column if condition is not met.
    update_func = when(
        col(Colname.energy_supplier_id) == col(sys_cor_energy_supplier),
        col(Colname.sum_quantity) + col(Colname.negative_grid_loss),
    ).otherwise(col(Colname.sum_quantity))

    result_df = (
        df.withColumn(adjusted_sum_quantity, update_func)
        .drop(Colname.sum_quantity)
        .withColumnRenamed(adjusted_sum_quantity, Colname.sum_quantity)
        .withColumnRenamed(Colname.aggregated_quality, Colname.quality)
    )

    result = result_df.select(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
        Colname.sum_quantity,
        Colname.quality,
        lit(MeteringPointType.production.value).alias(Colname.metering_point_type),
    ).orderBy(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
    )

    return create_dataframe_from_aggregation_result_schema(result)
