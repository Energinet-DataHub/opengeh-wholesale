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
import pyspark.sql.functions as f

from package.calculation.energy.schemas import aggregation_result_schema
from package.constants import Colname
from package.codelists import QuantityQuality

from package.common import assert_schema


def create_dataframe_from_aggregation_result_schema(result: DataFrame) -> DataFrame:
    """Fit result in a general DataFrame. This is used for all results and missing columns will be null."""

    # TODO BJM: Can some or all of this preprocessing be removed from this function?
    result = _add_missing_nullable_columns(result)
    # Replaces None value with zero for sum_quantity
    result = result.na.fill(value=0, subset=[Colname.sum_quantity])
    # Replaces None value with QuantityQuality.MISSING for quality
    result = result.na.fill(
        value=QuantityQuality.MISSING.value, subset=[Colname.quality]
    )
    result = result.drop(Colname.resolution)

    assert_schema(
        result.schema,
        aggregation_result_schema,
        ignore_column_order=True,
        ignore_nullability=True,
    )

    # Create data frame from RDD in order to be able to apply the schema
    return SparkSession.builder.getOrCreate().createDataFrame(
        result.select(
            Colname.grid_area,
            Colname.to_grid_area,
            Colname.from_grid_area,
            Colname.balance_responsible_id,
            Colname.energy_supplier_id,
            Colname.time_window,
            Colname.sum_quantity,
            f.col(Colname.quality).alias(Colname.qualities),
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.position,
        ).rdd,
        aggregation_result_schema,
    )


def _add_missing_nullable_columns(result: DataFrame) -> DataFrame:
    if Colname.to_grid_area not in result.columns:
        result = result.withColumn(Colname.to_grid_area, f.lit(None).cast("string"))
    if Colname.from_grid_area not in result.columns:
        result = result.withColumn(Colname.from_grid_area, f.lit(None))
    if Colname.balance_responsible_id not in result.columns:
        result = result.withColumn(Colname.balance_responsible_id, f.lit(None))
    if Colname.energy_supplier_id not in result.columns:
        result = result.withColumn(Colname.energy_supplier_id, f.lit(None))
    if Colname.settlement_method not in result.columns:
        result = result.withColumn(Colname.settlement_method, f.lit(None))
    if Colname.position not in result.columns:
        result = result.withColumn(Colname.position, f.lit(None))
    return result
