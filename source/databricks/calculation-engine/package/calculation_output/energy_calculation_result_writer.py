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
import pyspark.sql.types as t
from pyspark.sql.window import Window

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
        results: DataFrame,
        time_series_type: TimeSeriesType,
        aggregation_level: AggregationLevel,
    ) -> None:
        f"""
        Write one or more results to storage.
        The schema of the input data frame must match the schema {_write_input_schema}.
        Nullable columns are, however, optional.
        """
        results = self._add_nullable_columns_if_missing(results)

        # Assert schema after adding optional columns but before internal data frame transformations.
        # The order of the columns in the input data frame doesn't matter.
        assert_schema(
            results.schema,
            _write_input_schema,
            ignore_nullability=True,
            ignore_column_order=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )

        results = self._add_batch_columns(results)
        results = self._add_calculation_result_id(results)
        results = self._map_to_storage_dataframe(
            results, aggregation_level, time_series_type
        )

        self._write_to_storage(results)

    @staticmethod
    def _add_nullable_columns_if_missing(results: DataFrame) -> DataFrame:
        """
        Nullable columns may not be present in all data frames.
        Thus, they are added if missing.
        """
        for field in _write_input_schema:
            if field.nullable and field.name not in results.columns:
                results = results.withColumn(
                    field.name, f.lit(None).cast(field.dataType)
                )
        return results

    def _add_batch_columns(self, results):
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

    def _add_calculation_result_id(self, results):
        window = Window.partitionBy(self._get_column_group_for_calculation_result_id())
        results = results.withColumn(
            EnergyResultColumnNames.calculation_result_id,
            f.expr("uuid()").over(window),
        )
        return results

    @staticmethod
    def _map_to_storage_dataframe(results, aggregation_level, time_series_type):
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
            f.col(Colname.sum_quantity).alias(EnergyResultColumnNames.quantity),
            f.array(Colname.quality).alias(EnergyResultColumnNames.quantity_qualities),
            f.col(Colname.time_window_start).alias(EnergyResultColumnNames.time),
            f.lit(aggregation_level.value).alias(
                EnergyResultColumnNames.aggregation_level
            ),
            f.lit(time_series_type.value).alias(
                EnergyResultColumnNames.time_series_type
            ),
            f.col(EnergyResultColumnNames.calculation_id),
            f.col(EnergyResultColumnNames.calculation_type),
            f.col(EnergyResultColumnNames.calculation_execution_time_start),
            f.col(Colname.from_grid_area).alias(EnergyResultColumnNames.from_grid_area),
            f.col(EnergyResultColumnNames.calculation_result_id),
        )

    @staticmethod
    def _get_column_group_for_calculation_result_id() -> list[str]:
        """Get the columns that are required in order to define a single calculation result."""
        return [
            EnergyResultColumnNames.calculation_id,
            EnergyResultColumnNames.calculation_execution_time_start,  # TODO BJM: Not needed?
            EnergyResultColumnNames.calculation_type,  # TODO BJM: Not needed?
            EnergyResultColumnNames.grid_area,
            EnergyResultColumnNames.time_series_type,
            EnergyResultColumnNames.aggregation_level,
            EnergyResultColumnNames.from_grid_area,  # TODO BJM: Missing to_grid_area?
            EnergyResultColumnNames.balance_responsible_id,
            EnergyResultColumnNames.energy_supplier_id,
        ]

    @staticmethod
    def _write_to_storage(df):
        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(f"{OUTPUT_DATABASE_NAME}.{ENERGY_RESULT_TABLE_NAME}")


_write_input_schema = t.StructType(
    [
        # The grid area in question. In case of exchange it's the to-grid area.
        t.StructField(Colname.grid_area, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), True),
        t.StructField(Colname.balance_responsible_id, t.StringType(), True),
        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534
        t.StructField(Colname.quantity, t.DecimalType(18, 3), True),
        t.StructField(Colname.quality, t.StringType(), False),
        t.StructField(
            Colname.time_window,
            (
                t.StructType()
                .add(Colname.start, t.TimestampType())
                .add(Colname.end, t.TimestampType())
            ),
            False,
        ),
        t.StructField(Colname.aggregation_level, t.StringType(), False),
        t.StructField(Colname.time_series_type, t.StringType(), False),
        t.StructField(Colname.from_grid_area, t.StringType(), True),
    ]
)
"""
Results data frame that is to be written must match this schema.
Nullable columns are, however, allowed to be omitted from the results data frame.
The writer will add them before writing to storage.
"""
