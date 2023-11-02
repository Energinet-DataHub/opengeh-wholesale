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

from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when, lit, coalesce

from package.calculation.energy.energy_results import EnergyResults
from package.codelists import MeteringPointType
from package.constants import Colname

grid_loss_responsible_energy_supplier = "GridLossResponsible_EnergySupplier"
grid_loss_responsible_grid_area = "GridLossResponsible_GridArea"
adjusted_sum_quantity = "adjusted_sum_quantity"


# step 11
def adjust_production(
    production_result_df: EnergyResults,
    negative_grid_loss_result_df: EnergyResults,
    grid_loss_responsible_df: DataFrame,
) -> EnergyResults:
    return _apply_grid_loss_adjustment(
        production_result_df,
        negative_grid_loss_result_df,
        grid_loss_responsible_df,
        Colname.is_negative_grid_loss_responsible,
        MeteringPointType.PRODUCTION.value,
    )


# step 10
def adjust_flex_consumption(
    flex_consumption_result_df: EnergyResults,
    positive_grid_loss_result_df: EnergyResults,
    grid_loss_responsible_df: DataFrame,
) -> EnergyResults:
    return _apply_grid_loss_adjustment(
        flex_consumption_result_df,
        positive_grid_loss_result_df,
        grid_loss_responsible_df,
        Colname.is_positive_grid_loss_responsible,
        MeteringPointType.CONSUMPTION.value,
    )


def _apply_grid_loss_adjustment(
    results: EnergyResults,
    grid_loss_result_df: EnergyResults,
    grid_loss_responsible_df: DataFrame,
    grid_loss_responsible_type_col: str,
    metering_point_type: str,
) -> EnergyResults:
    # select columns from dataframe that contains information about metering points registered as negative or positive grid loss to use in join.
    glr_df = grid_loss_responsible_df.select(
        Colname.from_date,
        Colname.to_date,
        col(Colname.energy_supplier_id).alias(grid_loss_responsible_energy_supplier),
        col(Colname.grid_area).alias(grid_loss_responsible_grid_area),
        grid_loss_responsible_type_col,
    )

    # join result dataframes from previous steps on time window and grid area.
    df = results.df.join(
        grid_loss_result_df.df,
        [Colname.time_window, Colname.grid_area],
        "inner",
    ).select(
        results.df[Colname.grid_area],
        results.df[Colname.balance_responsible_id],
        results.df[Colname.energy_supplier_id],
        results.df[Colname.time_window],
        results.df[Colname.sum_quantity],
        results.df[Colname.qualities],
        grid_loss_result_df.df[Colname.sum_quantity].alias("grid_loss_sum_quantity"),
    )

    # join information from negative or positive grid loss dataframe on to joined result dataframe with information about which energy supplier,
    # that is responsible for grid loss in the given time window from the joined result dataframe.
    df = df.join(
        glr_df,
        when(
            col(Colname.to_date).isNotNull(),
            col(Colname.time_window_start) <= col(Colname.to_date),
        ).otherwise(True)
        & (col(Colname.time_window_start) >= col(Colname.from_date))
        & (
            col(Colname.to_date).isNull()
            | (col(Colname.time_window_end) <= col(Colname.to_date))
        )
        & (col(Colname.grid_area) == col(grid_loss_responsible_grid_area))
        & (col(grid_loss_responsible_type_col)),
        "left",
    )

    # update function that selects the sum of two columns if condition is met, or selects data from a single column if condition is not met.
    update_func = coalesce(
        when(
            col(Colname.energy_supplier_id)
            == col(grid_loss_responsible_energy_supplier),
            col(Colname.sum_quantity) + col("grid_loss_sum_quantity"),
        ),
        col(Colname.sum_quantity),
    )

    result_df = df.withColumn(Colname.sum_quantity, update_func)

    result = result_df.select(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
        Colname.sum_quantity,
        Colname.qualities,
        lit(metering_point_type).alias(Colname.metering_point_type),
    ).orderBy(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
    )

    return EnergyResults(result)
