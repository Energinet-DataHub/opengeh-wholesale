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

import pyspark.sql.functions as f
from pyspark.sql import DataFrame
from pyspark.sql.types import DecimalType

from package.calculation.calculator_args import CalculatorArgs
from package.calculation.energy.data_structures.energy_results import EnergyResults
from package.calculation.energy.resolution_transition_factory import (
    get_resolution,
)
from package.calculation.output.add_meta_data import add_metadata
from package.codelists import TimeSeriesType, AggregationLevel
from package.constants import Colname, EnergyResultColumnNames


def create(
    args: CalculatorArgs,
    energy_results: EnergyResults,
    time_series_type: TimeSeriesType,
    aggregation_level: AggregationLevel,
) -> DataFrame:

    df = _add_aggregation_level_and_time_series_type(
        energy_results.df, aggregation_level, time_series_type
    )
    df = add_metadata(args, _get_column_group_for_calculation_result_id(), df)
    metering_point_resolution = get_resolution(
        args.quarterly_resolution_transition_datetime,
        args.calculation_period_end_datetime,
    )
    df = df.withColumn(Colname.resolution, f.lit(metering_point_resolution.value))
    df = _map_to_storage_dataframe(df)

    return df


def _add_aggregation_level_and_time_series_type(
    results: DataFrame,
    aggregation_level: AggregationLevel,
    time_series_type: TimeSeriesType,
) -> DataFrame:
    return results.withColumn(
        EnergyResultColumnNames.aggregation_level,
        f.lit(aggregation_level.value),
    ).withColumn(
        EnergyResultColumnNames.time_series_type, f.lit(time_series_type.value)
    )


def _get_column_group_for_calculation_result_id() -> list[str]:
    """
    Get the columns that are required in order to define a single calculation result.

    Calculation metadata is not included as it is the same for all rows in the data frame being written.
    Metadata is: calculation_id, calculation_execution_time_start, calculation_type

    Time series type and aggregation level is the same for all rows (applied in the writer itself)
    and are thus neither part of this list.
    """
    return [
        Colname.grid_area,
        Colname.from_grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
    ]


def _map_to_storage_dataframe(results: DataFrame) -> DataFrame:
    """
    Map column names to the Delta table field names
    Note: The order of the columns must match the order of the columns in the Delta table
    """
    return results.select(
        f.col(Colname.grid_area).alias(EnergyResultColumnNames.grid_area),
        f.col(Colname.energy_supplier_id).alias(
            EnergyResultColumnNames.energy_supplier_id
        ),
        f.col(Colname.balance_responsible_id).alias(
            EnergyResultColumnNames.balance_responsible_id
        ),
        f.col(Colname.quantity)
        .alias(EnergyResultColumnNames.quantity)
        .cast(DecimalType(18, 3)),
        f.col(Colname.qualities).alias(EnergyResultColumnNames.quantity_qualities),
        f.col(Colname.observation_time).alias(EnergyResultColumnNames.time),
        f.col(EnergyResultColumnNames.aggregation_level),
        f.col(EnergyResultColumnNames.time_series_type),
        f.col(EnergyResultColumnNames.calculation_id),
        f.col(EnergyResultColumnNames.calculation_type),
        f.col(EnergyResultColumnNames.calculation_execution_time_start),
        f.col(Colname.from_grid_area).alias(EnergyResultColumnNames.from_grid_area),
        f.col(EnergyResultColumnNames.calculation_result_id),
        f.col(EnergyResultColumnNames.metering_point_id),
        f.col(EnergyResultColumnNames.resolution),
    )
