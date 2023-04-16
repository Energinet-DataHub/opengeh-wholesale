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
from delta.tables import DeltaTable
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, lit
from pyspark.sql import SparkSession

import package.infrastructure as infra
from package.codelists import MarketRole, TimeSeriesType, AggregationLevel
from package.constants import Colname, PartitionKeyName
from package.schemas import results_schema, ResultSchemaField, constraints

DATABASE_NAME = "wholesale_output"
RESULT_TABLE_NAME = "result"


class ProcessStepResultWriter:
    def __init__(
        self,
        spark: SparkSession,
        container_path: str,
        batch_id: str,
        batch_process_type: str,
        batch_execution_time_start: datetime,
    ):
        self.__batch_id = batch_id
        self.__batch_process_type = batch_process_type
        self.__batch_execution_time_start = batch_execution_time_start
        self.__output_path = (
            f"{container_path}/{infra.get_batch_relative_path(batch_id)}"
        )
        self._create_delta_table_if_not_exists(spark, container_path)

    def write(
        self,
        result_df: DataFrame,
        time_series_type: TimeSeriesType,
        aggregation_level: AggregationLevel,
    ) -> None:
        # start: write to delta table (before dropping columns)
        self._write_result_to_table(result_df, aggregation_level, time_series_type)
        # end: write to delta table

        result_df = self._prepare_result_for_output(
            result_df, time_series_type, aggregation_level
        )

        if aggregation_level == AggregationLevel.total_ga:
            self._write_per_ga(result_df)
        elif aggregation_level == AggregationLevel.es_per_ga:
            self._write_per_ga_per_actor(result_df, MarketRole.ENERGY_SUPPLIER)
        elif aggregation_level == AggregationLevel.brp_per_ga:
            self._write_per_ga_per_actor(
                result_df, MarketRole.BALANCE_RESPONSIBLE_PARTY
            )
        elif aggregation_level == AggregationLevel.es_per_brp_per_ga:
            self._write_per_ga_per_brp_per_es(result_df)
        else:
            raise ValueError(
                f"Unsupported aggregation_level, {aggregation_level.value}"
            )

    def _write_per_ga(
        self,
        result_df: DataFrame,
    ) -> None:
        result_df.drop(Colname.energy_supplier_id).drop(Colname.balance_responsible_id)
        partition_by = [
            PartitionKeyName.GROUPING,
            PartitionKeyName.TIME_SERIES_TYPE,
            PartitionKeyName.GRID_AREA,
        ]
        self._write_result_df(result_df, partition_by)

    def _write_per_ga_per_actor(
        self,
        result_df: DataFrame,
        market_role: MarketRole,
    ) -> None:
        result_df = self._add_gln(result_df, market_role)
        result_df.drop(Colname.energy_supplier_id).drop(Colname.balance_responsible_id)
        partition_by = [
            PartitionKeyName.GROUPING,
            PartitionKeyName.TIME_SERIES_TYPE,
            PartitionKeyName.GRID_AREA,
            PartitionKeyName.GLN,
        ]
        self._write_result_df(result_df, partition_by)

    def _write_per_ga_per_brp_per_es(
        self,
        result_df: DataFrame,
    ) -> None:
        result_df = result_df.withColumnRenamed(
            Colname.balance_responsible_id,
            PartitionKeyName.BALANCE_RESPONSIBLE_PARTY_GLN,
        ).withColumnRenamed(
            Colname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
        )

        partition_by = [
            PartitionKeyName.GROUPING,
            PartitionKeyName.TIME_SERIES_TYPE,
            PartitionKeyName.GRID_AREA,
            PartitionKeyName.BALANCE_RESPONSIBLE_PARTY_GLN,
            PartitionKeyName.ENERGY_SUPPLIER_GLN,
        ]
        self._write_result_df(result_df, partition_by)

    def _prepare_result_for_output(
        self,
        result_df: DataFrame,
        time_series_type: TimeSeriesType,
        aggregation_level: AggregationLevel,
    ) -> DataFrame:
        result_df = result_df.select(
            col(Colname.grid_area).alias(PartitionKeyName.GRID_AREA),
            Colname.energy_supplier_id,
            Colname.balance_responsible_id,
            col(Colname.sum_quantity).alias("quantity").cast("string"),
            col(Colname.quality).alias("quality"),
            col(Colname.time_window_start).alias("quarter_time"),
        )

        result_df = result_df.withColumn(
            PartitionKeyName.GROUPING, lit(aggregation_level.value)
        ).withColumn(PartitionKeyName.TIME_SERIES_TYPE, lit(time_series_type.value))

        return result_df

    def _add_gln(
        self,
        result_df: DataFrame,
        market_role: MarketRole,
    ) -> DataFrame:
        if market_role is MarketRole.ENERGY_SUPPLIER:
            result_df = result_df.withColumnRenamed(
                Colname.energy_supplier_id, Colname.gln
            )
        elif market_role is MarketRole.BALANCE_RESPONSIBLE_PARTY:
            result_df = result_df.withColumnRenamed(
                Colname.balance_responsible_id, Colname.gln
            )
        else:
            raise NotImplementedError(
                f"Market role, {market_role}, is not supported yet"
            )

        return result_df

    def _write_result_df(
        self,
        result_df: DataFrame,
        partition_by: list[str],
    ) -> None:
        result_data_directory = f"{self.__output_path}/result/"

        # First repartition to co-locate all rows for a grid area on a single executor.
        # This ensures that only one file is being written/created for each grid area
        # When writing/creating the files. The partition by creates a folder for each grid area.
        (
            result_df.repartition(PartitionKeyName.GRID_AREA)
            .write.mode("append")
            .partitionBy(partition_by)
            .json(result_data_directory)
        )

    def _create_delta_table_if_not_exists(
        self, spark: SparkSession, container_path: str
    ) -> None:
        db_location = f"{container_path}/{infra.get_calculation_output_folder()}"
        table_location = (
            f"{container_path}/{infra.get_calculation_output_folder()}/result"
        )

        if DeltaTable.isDeltaTable(spark, table_location):
            return

        # First create database if not already existing
        spark.sql(
            f"CREATE DATABASE IF NOT EXISTS {DATABASE_NAME} \
            COMMENT 'Contains result data from wholesale domain.' \
            LOCATION '{db_location}'"
        )

        # Now create table if not already existing
        (
            DeltaTable.createIfNotExists(spark)
            .tableName(f"{DATABASE_NAME}.{RESULT_TABLE_NAME}")
            .location(table_location)
            .addColumns(results_schema)
            .execute()
        )
        # Add constraints
        for constraint in constraints:
            spark.sql(
                f"ALTER TABLE {DATABASE_NAME}.{RESULT_TABLE_NAME} ADD CONSTRAINT {constraint[0]} CHECK ({constraint[1]})"
            )

    def _write_result_to_table(
        self,
        df: DataFrame,
        aggregation_level: AggregationLevel,
        time_series_type: TimeSeriesType,
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
        df = df.select(
            col(Colname.batch_id).alias(ResultSchemaField.batch_id),
            col(Colname.batch_execution_time_start).alias(
                ResultSchemaField.batch_execution_time_start
            ),
            col(Colname.batch_process_type).alias(ResultSchemaField.batch_process_type),
            lit(time_series_type.value).alias(ResultSchemaField.time_series_type),
            col(Colname.grid_area).alias(ResultSchemaField.grid_area),
            col(Colname.out_grid_area).alias(ResultSchemaField.out_grid_area),
            col(Colname.balance_responsible_id).alias(
                ResultSchemaField.balance_responsible_id
            ),
            col(Colname.energy_supplier_id).alias(ResultSchemaField.energy_supplier_id),
            col(Colname.time_window_start).alias(ResultSchemaField.time),
            col(Colname.sum_quantity).alias(ResultSchemaField.quantity),
            col(Colname.quality).alias(ResultSchemaField.quantity_quality),
            lit(aggregation_level.value).alias(ResultSchemaField.aggregation_level),
        )

        df.write.format("delta").mode("append").option("mergeSchema", "false").option(
            "mergeSchema", "false"
        ).saveAsTable(f"{DATABASE_NAME}.{RESULT_TABLE_NAME}")
