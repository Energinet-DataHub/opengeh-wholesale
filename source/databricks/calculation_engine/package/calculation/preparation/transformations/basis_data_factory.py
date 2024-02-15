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
from pyspark.sql.functions import col

from package.calculation.CalculationResults import BasisDataContainer
from package.calculation.preparation.transformations import basis_data
from package.constants import BasisDataColname, PartitionKeyName
from package.infrastructure import logging_configuration


def create(
    metering_point_periods_df: DataFrame,
    metering_point_time_series_df: DataFrame,
    time_zone: str,
) -> BasisDataContainer:
    basis_data_container = BasisDataContainer()

    with logging_configuration.start_span("basis_data_prepare"):
        (
            timeseries_quarter_df,
            timeseries_hour_df,
        ) = basis_data.get_metering_point_time_series_basis_data_dfs(
            metering_point_time_series_df, time_zone
        )

        master_basis_data_df = basis_data.get_master_basis_data_df(
            metering_point_periods_df
        )

        # Get basis data for energy suppliers
        (
            master_basis_data,
            time_series_quarter_basis_data_df,
            time_series_hour_basis_data_df,
        ) = _get_es_basis_data(
            master_basis_data_df, timeseries_quarter_df, timeseries_hour_df
        )

        basis_data_container.time_series_hour_basis_data_per_es_per_ga = (
            time_series_hour_basis_data_df
        )
        basis_data_container.time_series_quarter_basis_data_per_es_per_ga = (
            time_series_quarter_basis_data_df
        )
        basis_data_container.master_basis_data_per_es_per_ga = master_basis_data_df

        # Add basis data for total grid area
        (
            master_basis_data_df,
            time_series_quarter_basis_data_df,
            time_series_hour_basis_data_df,
        ) = _get_ga_basis_data(
            master_basis_data_df, timeseries_quarter_df, timeseries_hour_df
        )

        basis_data_container.time_series_hour_basis_data = (
            time_series_hour_basis_data_df
        )
        basis_data_container.time_series_quarter_basis_data_per_total_ga = (
            time_series_quarter_basis_data_df
        )
        basis_data_container.master_basis_data_per_total_ga = master_basis_data_df

    return basis_data_container


@logging_configuration.use_span("per_grid_area")
def _get_ga_basis_data(
    master_basis_data_df: DataFrame,
    timeseries_quarter_df: DataFrame,
    timeseries_hour_df: DataFrame,
) -> tuple[DataFrame, DataFrame, DataFrame]:

    timeseries_quarter_df = timeseries_quarter_df.drop(
        BasisDataColname.energy_supplier_id
    ).withColumnRenamed(BasisDataColname.grid_area, PartitionKeyName.GRID_AREA)

    timeseries_hour_df = timeseries_hour_df.drop(
        BasisDataColname.energy_supplier_id
    ).withColumnRenamed(BasisDataColname.grid_area, PartitionKeyName.GRID_AREA)

    master_basis_data_df = master_basis_data_df.withColumn(
        PartitionKeyName.GRID_AREA, col(BasisDataColname.grid_area)
    )

    return (
        master_basis_data_df,
        timeseries_quarter_df,
        timeseries_hour_df,
    )


@logging_configuration.use_span("per_energy_supplier")
def _get_es_basis_data(
    master_basis_data_df: DataFrame,
    timeseries_quarter_df: DataFrame,
    timeseries_hour_df: DataFrame,
) -> tuple[DataFrame, DataFrame, DataFrame]:

    timeseries_quarter_df = timeseries_quarter_df.withColumnRenamed(
        BasisDataColname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
    ).withColumnRenamed(BasisDataColname.grid_area, PartitionKeyName.GRID_AREA)

    timeseries_hour_df = timeseries_hour_df.withColumnRenamed(
        BasisDataColname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
    ).withColumnRenamed(BasisDataColname.grid_area, PartitionKeyName.GRID_AREA)

    master_basis_data_df = master_basis_data_df.withColumnRenamed(
        BasisDataColname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
    ).withColumn(PartitionKeyName.GRID_AREA, col(BasisDataColname.grid_area))

    return (
        master_basis_data_df,
        timeseries_quarter_df,
        timeseries_hour_df,
    )
