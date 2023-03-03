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
from pyspark.sql import DataFrame, SparkSession

from pyspark.sql.functions import lit
from package.shared.data_classes import Metadata
from package.constants import Colname
from package.schemas.output import aggregation_result_schema
from package.codelists import TimeSeriesQuality


def __add_missing_nullable_columns(result: DataFrame) -> DataFrame:
    if Colname.in_grid_area not in result.columns:
        result = result.withColumn(Colname.in_grid_area, lit(None))
    if Colname.out_grid_area not in result.columns:
        result = result.withColumn(Colname.out_grid_area, lit(None))
    if Colname.balance_responsible_id not in result.columns:
        result = result.withColumn(Colname.balance_responsible_id, lit(None))
    if Colname.energy_supplier_id not in result.columns:
        result = result.withColumn(Colname.energy_supplier_id, lit(None))
    if Colname.settlement_method not in result.columns:
        result = result.withColumn(Colname.settlement_method, lit(None))
    if Colname.added_grid_loss not in result.columns:
        result = result.withColumn(Colname.added_grid_loss, lit(None))
    if Colname.added_system_correction not in result.columns:
        result = result.withColumn(Colname.added_system_correction, lit(None))
    if Colname.position not in result.columns:
        result = result.withColumn(Colname.position, lit(None))
    return result


def create_dataframe_from_aggregation_result_schema(
    metadata: Metadata, result: DataFrame
) -> DataFrame:
    "Fit result in a general DataFrame. This is used for all results and missing columns will be null."

    result = __add_missing_nullable_columns(result)
    # Replaces None value with zero for sum_quantity
    result = result.na.fill(value=0, subset=[Colname.sum_quantity])
    # Replaces None value with TimeSeriesQuality.missing for quality
    result = result.na.fill(
        value=TimeSeriesQuality.missing.value, subset=[Colname.quality]
    )

    # Create data frame from RDD in order to be able to apply the schema
    return SparkSession.builder.getOrCreate().createDataFrame(
        result.select(
            Colname.grid_area,
            Colname.in_grid_area,
            Colname.out_grid_area,
            Colname.balance_responsible_id,
            Colname.energy_supplier_id,
            Colname.time_window,
            Colname.sum_quantity,
            Colname.quality,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.added_grid_loss,
            Colname.added_system_correction,
            Colname.position,
        ).rdd,
        aggregation_result_schema,
    )
