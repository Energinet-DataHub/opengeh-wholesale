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


def apply_grid_loss_adjustment(
    results: EnergyResults,
    grid_loss_result: EnergyResults,
    grid_loss_responsible: GridLossResponsible,
    metering_point_type: MeteringPointType,
) -> EnergyResults:
    result_df = results.df
    grid_loss_result_df = grid_loss_result.df

    grid_loss_responsible_df = grid_loss_responsible.df.select(
        Colname.from_date,
        Colname.to_date,
        col(Colname.energy_supplier_id).alias(grid_loss_responsible_energy_supplier),
        col(Colname.grid_area).alias(grid_loss_responsible_grid_area),
        Colname.metering_point_type,
    )

    joined_grid_loss_result_and_responsible = grid_loss_result_df.join(
        grid_loss_responsible_df,
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
        & (col(Colname.metering_point_type) == metering_point_type.value),
        "left",
    ).select(
        Colname.grid_area,
        col(grid_loss_responsible_energy_supplier).alias(Colname.energy_supplier_id),
        Colname.time_window,
        Colname.sum_quantity,
        Colname.qualities,
        Colname.metering_point_type,
    )

    df = (
        result_df.join(
            joined_grid_loss_result_and_responsible,
            [Colname.time_window, Colname.grid_area, Colname.energy_supplier_id],
            "outer",
        )
        .select(
            Colname.grid_area,
            result_df[Colname.balance_responsible_id],
            coalesce(
                result_df[Colname.energy_supplier_id],
                joined_grid_loss_result_and_responsible[Colname.energy_supplier_id],
            ).alias(Colname.energy_supplier_id),
            Colname.time_window,
            result_df[Colname.sum_quantity],
            when(
                result_df[Colname.qualities].isNull(),
                joined_grid_loss_result_and_responsible[Colname.qualities],
            )
            .when(
                joined_grid_loss_result_and_responsible[Colname.qualities].isNull(),
                result_df[Colname.qualities],
            )
            .otherwise(
                array_union(
                    result_df[Colname.qualities], grid_loss_result_df[Colname.qualities]
                )
            )
            .alias(Colname.qualities),
            joined_grid_loss_result_and_responsible[Colname.sum_quantity].alias(
                "grid_loss_sum_quantity"
            ),
        )
        .na.fill(0, subset=["grid_loss_sum_quantity", Colname.sum_quantity])
    )

    result_df = df.withColumn(
        adjusted_sum_quantity, col(Colname.sum_quantity) + col("grid_loss_sum_quantity")
    )

    result = result_df.select(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
        col(adjusted_sum_quantity).alias(Colname.sum_quantity),
        Colname.qualities,
    ).orderBy(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
    )

    return EnergyResults(result)
