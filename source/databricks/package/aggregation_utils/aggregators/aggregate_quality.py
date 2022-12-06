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

from geh_stream.codelists import Colname
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when, window, count, year, month, dayofmonth, hour
from geh_stream.codelists import Quality


temp_estimated_quality_count = "temp_estimated_quality_count"
temp_quantity_missing_quality_count = "temp_quantity_missing_quality_count"

aggregated_production_quality = "aggregated_production_quality"
aggregated_net_exchange_quality = "aggregated_net_exchange_quality"


def aggregate_quality(time_series_df: DataFrame):
    agg_df = time_series_df.groupBy(Colname.grid_area, Colname.metering_point_type, window(Colname.time, "1 hour")) \
        .agg(
            # Count entries where quality is estimated (Quality=56)
            count(when(col(Colname.quality) == Quality.estimated.value, 1)).alias(temp_estimated_quality_count),
            # Count entries where quality is quantity missing (Quality=QM)
            count(when(col(Colname.quality) == Quality.quantity_missing.value, 1)).alias(temp_quantity_missing_quality_count)
            ) \
        .withColumn(
                    Colname.aggregated_quality,
                    (
                    # Set quality to as read (Quality=E01) if no entries where quality is estimated or quantity missing
                    when(col(temp_estimated_quality_count) > 0, Quality.estimated.value)
                    .when(col(temp_quantity_missing_quality_count) > 0, Quality.estimated.value)
                    .otherwise(Quality.as_read.value)
                    )
                    ) \
        .drop(temp_estimated_quality_count) \
        .drop(temp_quantity_missing_quality_count) \
        .withColumn(Colname.time, col("window").start) \
        .withColumnRenamed("window", Colname.time_window)

    joined_df = time_series_df \
        .join(agg_df,
              (year(time_series_df[Colname.time]) == year(agg_df[Colname.time]))
              & (month(time_series_df[Colname.time]) == month(agg_df[Colname.time]))
              & (dayofmonth(time_series_df[Colname.time]) == dayofmonth(agg_df[Colname.time]))
              & (hour(time_series_df[Colname.time]) == hour(agg_df[Colname.time]))
              & (time_series_df[Colname.metering_point_type] == agg_df[Colname.metering_point_type])
              & (time_series_df[Colname.grid_area] == agg_df[Colname.grid_area]), "inner") \
        .select(time_series_df["*"], agg_df[Colname.aggregated_quality])
    return joined_df


def aggregate_total_consumption_quality(df: DataFrame):
    df = df.groupBy(Colname.grid_area, Colname.time_window, Colname.sum_quantity) \
        .agg(
            # Count entries where quality is estimated (Quality=56)
            count(
                when(col(aggregated_production_quality) == Quality.estimated.value, 1) \
                .when(col(aggregated_net_exchange_quality) == Quality.estimated.value, 1)) \
            .alias(temp_estimated_quality_count),
            # Count entries where quality is quantity missing (Quality=QM)
            count(
                when(col(aggregated_production_quality) == Quality.quantity_missing.value, 1) \
                .when(col(aggregated_net_exchange_quality) == Quality.quantity_missing.value, 1)) \
            .alias(temp_quantity_missing_quality_count)
            ) \
        .withColumn(
                    Colname.quality,
                    (
                        # Set quality to as read (Quality=E01) if no entries where quality is estimated or quantity missing
                        when(col(temp_estimated_quality_count) > 0, Quality.estimated.value)
                        .when(col(temp_quantity_missing_quality_count) > 0, Quality.estimated.value)
                        .otherwise(Quality.as_read.value)
                    )
                   )
    return df
