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

from pyspark.sql.functions import col, when, lit, array_union, coalesce

from package.calculation.energy.energy_results import EnergyResults
from package.codelists import MeteringPointType
from package.constants import Colname
from package.calculation.preparation.grid_loss_responsible import GridLossResponsible

grid_loss_responsible_energy_supplier = "GridLossResponsible_EnergySupplier"
grid_loss_responsible_grid_area = "GridLossResponsible_GridArea"
adjusted_sum_quantity = "adjusted_sum_quantity"


# step 11
def adjust_production(
    production_result_df: EnergyResults,
    negative_grid_loss_result_df: EnergyResults,
    grid_loss_responsible_df: GridLossResponsible,
) -> EnergyResults:
    return _apply_grid_loss_adjustment(
        production_result_df,
        negative_grid_loss_result_df,
        grid_loss_responsible_df,
        Colname.is_negative_grid_loss_responsible,
    )


# step 10
def adjust_flex_consumption(
    flex_consumption_result_df: EnergyResults,
    positive_grid_loss_result_df: EnergyResults,
    grid_loss_responsible_df: GridLossResponsible,
) -> EnergyResults:
    return _apply_grid_loss_adjustment(
        flex_consumption_result_df,
        positive_grid_loss_result_df,
        grid_loss_responsible_df,
        Colname.is_positive_grid_loss_responsible,
    )


def _apply_grid_loss_adjustment(
    results: EnergyResults,
    grid_loss_result: EnergyResults,
    grid_loss_responsible: GridLossResponsible,
    grid_loss_responsible_type_col: str,
) -> EnergyResults:
    # select columns from dataframe that contains information about metering points registered as negative or positive grid loss to use in join.
    glr_df = grid_loss_responsible.df.select(
        Colname.from_date,
        Colname.to_date,
        col(Colname.energy_supplier_id).alias(grid_loss_responsible_energy_supplier),
        col(Colname.grid_area).alias(grid_loss_responsible_grid_area),
        grid_loss_responsible_type_col,
    )
    grid_loss_result_df = grid_loss_result.df.drop(
        Colname.energy_supplier_id, Colname.balance_responsible_id
    )
    # join result dataframes from previous steps on time window and grid area.
    df = grid_loss_result_df.join(
        results.df, [Colname.time_window, Colname.grid_area], "left"
    ).select(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
        coalesce(results.df[Colname.sum_quantity], lit(0)).alias(Colname.sum_quantity),
        when(
            results.df[Colname.qualities].isNull(),
            grid_loss_result_df[Colname.qualities],
        )
        .otherwise(
            array_union(
                results.df[Colname.qualities], grid_loss_result_df[Colname.qualities]
            )
        )
        .alias(Colname.qualities),
        grid_loss_result_df[Colname.sum_quantity].alias("grid_loss_sum_quantity"),
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
    ).withColumn(
        Colname.energy_supplier_id,
        coalesce(
            col(Colname.energy_supplier_id),
            col(grid_loss_responsible_energy_supplier),
        ),
    )

    # update function that selects the sum of two columns if condition is met, or selects data from a single column if condition is not met.
    update_func = when(
        col(Colname.energy_supplier_id) == col(grid_loss_responsible_energy_supplier),
        col(Colname.sum_quantity) + col("grid_loss_sum_quantity"),
    ).otherwise(col(Colname.sum_quantity))

    result_df = (
        df.withColumn(adjusted_sum_quantity, update_func)
        .drop(Colname.sum_quantity)
        .withColumnRenamed(adjusted_sum_quantity, Colname.sum_quantity)
    )

    result = result_df.select(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
        Colname.sum_quantity,
        Colname.qualities,
    ).orderBy(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
    )

    return EnergyResults(result)
