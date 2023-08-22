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
from pyspark.sql.functions import col, lit, first
import pyspark.sql.functions as F
from pyspark.sql.window import Window

from package.codelists import TimeSeriesType, AggregationLevel, ProcessType
from package.constants import Colname, EnergyResultTableColName
from package.infrastructure import OUTPUT_DATABASE_NAME, ENERGY_RESULT_TABLE_NAME


class CalculationResultWriter:
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
        df: DataFrame,
        time_series_type: TimeSeriesType,
        aggregation_level: AggregationLevel,
    ) -> None:
        # Add columns that are the same for all calculation results in batch
        df = (
            df.withColumn(Colname.batch_id, lit(self.__batch_id))
            .withColumn(Colname.batch_process_type, lit(self.__batch_process_type))
            .withColumn(
                Colname.batch_execution_time_start,
                lit(self.__batch_execution_time_start),
            )
        )

        # Map column names to the Delta table field names
        # Note: The order of the columns must match the order of the columns in the Delta table
        df = df.select(
            col(Colname.grid_area).alias(EnergyResultTableColName.grid_area),
            col(Colname.energy_supplier_id).alias(
                EnergyResultTableColName.energy_supplier_id
            ),
            col(Colname.balance_responsible_id).alias(
                EnergyResultTableColName.balance_responsible_id
            ),
            col(Colname.sum_quantity).alias(EnergyResultTableColName.quantity),
            col(Colname.quality).alias(EnergyResultTableColName.quantity_quality),
            col(Colname.time_window_start).alias(EnergyResultTableColName.time),
            lit(aggregation_level.value).alias(EnergyResultTableColName.aggregation_level),
            lit(time_series_type.value).alias(EnergyResultTableColName.time_series_type),
            col(Colname.batch_id).alias(EnergyResultTableColName.batch_id),
            col(Colname.batch_process_type).alias(
                EnergyResultTableColName.batch_process_type
            ),
            col(Colname.batch_execution_time_start).alias(
                EnergyResultTableColName.batch_execution_time_start
            ),
            col(Colname.from_grid_area).alias(EnergyResultTableColName.from_grid_area),
        )

        df = df.withColumn(EnergyResultTableColName.calculation_result_id, F.expr("uuid()"))
        window = Window.partitionBy(_get_column_group_for_calculation_result_id())
        df = df.withColumn(EnergyResultTableColName.calculation_result_id, first(col(EnergyResultTableColName.calculation_result_id)).over(window))

        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(f"{OUTPUT_DATABASE_NAME}.{ENERGY_RESULT_TABLE_NAME}")


def _get_column_group_for_calculation_result_id() -> list[str]:
    return [EnergyResultTableColName.batch_id, EnergyResultTableColName.batch_execution_time_start, EnergyResultTableColName.batch_process_type,
            EnergyResultTableColName.grid_area, EnergyResultTableColName.time_series_type, EnergyResultTableColName.aggregation_level,
            EnergyResultTableColName.from_grid_area, EnergyResultTableColName.balance_responsible_id, EnergyResultTableColName.energy_supplier_id]
