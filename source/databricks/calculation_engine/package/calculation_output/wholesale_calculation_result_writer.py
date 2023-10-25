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
import pyspark.sql.functions as f
from pyspark.sql.window import Window

from package.codelists import (
    ProcessType,
)
from package.constants import Colname, WholesaleResultColumnNames
from package.infrastructure.paths import (
    OUTPUT_DATABASE_NAME,
    WHOLESALE_RESULT_TABLE_NAME,
)


class WholesaleCalculationResultWriter:
    def __init__(
        self,
        batch_id: str,
        process_type: ProcessType,
        execution_time_start: datetime,
    ):
        self.__batch_id = batch_id
        self.__process_type = process_type.value
        self.__execution_time_start = execution_time_start

    def write(
        self,
        df: DataFrame,
    ) -> None:
        df = self._add_metadata(df)
        df = self._add_calculation_result_id(df)
        df = self._select_output_columns(df)

        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(f"{OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}")

    def _add_metadata(self, df: DataFrame) -> DataFrame:
        return (
            df.withColumn(Colname.batch_id, lit(self.__batch_id))
            .withColumn(Colname.batch_process_type, lit(self.__process_type))
            .withColumn(
                Colname.batch_execution_time_start,
                lit(self.__execution_time_start),
            )
        )

    def _add_calculation_result_id(self, df: DataFrame) -> DataFrame:
        df = df.withColumn(
            WholesaleResultColumnNames.calculation_result_id, f.expr("uuid()")
        )
        window = Window.partitionBy(self._get_column_group_for_calculation_result_id())
        return df.withColumn(
            WholesaleResultColumnNames.calculation_result_id,
            first(col(WholesaleResultColumnNames.calculation_result_id)).over(window),
        )

    @staticmethod
    def _select_output_columns(df: DataFrame) -> DataFrame:
        # Map column names to the Delta table field names
        # Note: The order of the columns must match the order of the columns in the Delta table
        return df.select(
            col(Colname.batch_id).alias(WholesaleResultColumnNames.calculation_id),
            col(Colname.batch_process_type).alias(
                WholesaleResultColumnNames.calculation_type
            ),
            col(Colname.batch_execution_time_start).alias(
                WholesaleResultColumnNames.calculation_execution_time_start
            ),
            col(WholesaleResultColumnNames.calculation_result_id),
            col(Colname.grid_area).alias(WholesaleResultColumnNames.grid_area),
            col(Colname.energy_supplier_id).alias(
                WholesaleResultColumnNames.energy_supplier_id
            ),
            col(Colname.total_quantity).alias(WholesaleResultColumnNames.quantity),
            col(Colname.unit).alias(WholesaleResultColumnNames.quantity_unit),
            col(Colname.qualities).alias(WholesaleResultColumnNames.quantity_qualities),
            col(Colname.charge_time).alias(WholesaleResultColumnNames.time),
            col(Colname.charge_resolution).alias(WholesaleResultColumnNames.resolution),
            col(Colname.metering_point_type).alias(
                WholesaleResultColumnNames.metering_point_type
            ),
            col(Colname.settlement_method).alias(
                WholesaleResultColumnNames.settlement_method
            ),
            col(Colname.charge_price).alias(WholesaleResultColumnNames.price),
            col(Colname.total_amount).alias(WholesaleResultColumnNames.amount),
            col(Colname.charge_tax).alias(WholesaleResultColumnNames.is_tax),
            col(Colname.charge_code).alias(WholesaleResultColumnNames.charge_code),
            col(Colname.charge_type).alias(WholesaleResultColumnNames.charge_type),
            col(Colname.charge_owner).alias(WholesaleResultColumnNames.charge_owner_id),
        )

    @staticmethod
    def _get_column_group_for_calculation_result_id() -> list[str]:
        """
        This column group defines the a calculation result. Metadata columns, e.g., those that are set inside this class,
        are not included a part of this group, since they are not expected to influence the result id
        """
        return [
            Colname.batch_id,
            Colname.charge_resolution,
            Colname.charge_type,
            Colname.grid_area,
            Colname.charge_owner,
            Colname.energy_supplier_id,
            Colname.metering_point_type,
            Colname.settlement_method,
        ]
