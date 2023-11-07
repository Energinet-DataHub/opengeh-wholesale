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

from datetime import datetime
from pyspark.sql import DataFrame
import pyspark.sql.functions as f
from pyspark.sql.window import Window

from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.codelists import TimeSeriesType, AggregationLevel, ProcessType
from package.common import assert_schema
from package.constants import Colname, EnergyResultColumnNames
from package.infrastructure.paths import OUTPUT_DATABASE_NAME, ENERGY_RESULT_TABLE_NAME


class EnergyCalculationResultWriter:
    def __init__(
        self,
        batch_id: str,
        batch_process_type: ProcessType,
        batch_execution_time_start: datetime,
    ):
        self.__batch_id = batch_id
        self.__batch_process_type = batch_process_type.value
        self.__batch_execution_time_start = batch_execution_time_start

    def write(
        self,
        results: EnergyResults,
        time_series_type: TimeSeriesType,
        aggregation_level: AggregationLevel,
    ) -> None:
        f"""
        Write one or more results to storage.
        The schema of the input data frame must match the schema {energy_results_schema}.
        Nullable columns are, however, optional.
        """
        # TODO BJM: Two schemas and duplicate adding nullable columns?
        results_df = self._add_nullable_columns_if_missing(results.df)

        # Assert schema after adding optional columns but before internal data frame transformations.
        # The order of the columns in the input data frame doesn't matter.
        assert_schema(
            results_df.schema,
            energy_results_schema,
            ignore_nullability=True,
            ignore_column_order=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )

        results_df = self._add_aggregation_level_and_time_series_type(
            results_df, aggregation_level, time_series_type
        )
        results_df = self._add_batch_columns(results_df)
        results_df = self._add_calculation_result_id(results_df)
        results_df = self._map_to_storage_dataframe(results_df)

        self._write_to_storage(results_df)

    @staticmethod
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

    @staticmethod
    def _add_nullable_columns_if_missing(results: DataFrame) -> DataFrame:
        """
        Nullable columns may not be present in all data frames.
        Thus, they are added if missing.
        """
        for field in energy_results_schema:
            if field.nullable and field.name not in results.columns:
                results = results.withColumn(
                    field.name, f.lit(None).cast(field.dataType)
                )
        return results

    def _add_batch_columns(self, results: DataFrame) -> DataFrame:
        """Add columns that are the same for all calculation results in batch."""
        return (
            results.withColumn(
                EnergyResultColumnNames.calculation_id, f.lit(self.__batch_id)
            )
            .withColumn(
                EnergyResultColumnNames.calculation_type,
                f.lit(self.__batch_process_type),
            )
            .withColumn(
                EnergyResultColumnNames.calculation_execution_time_start,
                f.lit(self.__batch_execution_time_start),
            )
        )

    def _add_calculation_result_id(self, results: DataFrame) -> DataFrame:
        results = results.withColumn(
            EnergyResultColumnNames.calculation_result_id, f.expr("uuid()")
        )
        window = Window.partitionBy(self._get_column_group_for_calculation_result_id())
        results = results.withColumn(
            EnergyResultColumnNames.calculation_result_id,
            f.first(f.col(EnergyResultColumnNames.calculation_result_id)).over(window),
        )
        return results

    @staticmethod
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
            # # TODO JVM: This is a temporary fix for the fact that the sum_quantity column is not nullable
            # f.coalesce(f.col(Colname.sum_quantity), f.lit(0)).alias(
            #     EnergyResultColumnNames.quantity
            # ),
            f.col(Colname.sum_quantity).alias(EnergyResultColumnNames.quantity),
            f.col(Colname.qualities).alias(EnergyResultColumnNames.quantity_qualities),
            f.col(Colname.time_window_start).alias(EnergyResultColumnNames.time),
            f.col(EnergyResultColumnNames.aggregation_level),
            f.col(EnergyResultColumnNames.time_series_type),
            f.col(EnergyResultColumnNames.calculation_id),
            f.col(EnergyResultColumnNames.calculation_type),
            f.col(EnergyResultColumnNames.calculation_execution_time_start),
            f.col(Colname.from_grid_area).alias(EnergyResultColumnNames.from_grid_area),
            f.col(EnergyResultColumnNames.calculation_result_id),
        )

    @staticmethod
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

    @staticmethod
    def _write_to_storage(results: DataFrame) -> None:
        results.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(f"{OUTPUT_DATABASE_NAME}.{ENERGY_RESULT_TABLE_NAME}")
