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

from package.schemas.eventhub_timeseries_schema import eventhub_timeseries_schema
from pyspark.sql.functions import (
    from_json,
    explode,
    when,
    col,
    to_timestamp,
    expr,
    year,
    month,
    dayofmonth,
    lit,
)
from pyspark.sql.dataframe import DataFrame
from package.codelists import Resolution
from package.codelists import Colname


def transform_unprocessed_time_series_to_points(source: DataFrame) -> DataFrame:
    "RegistrationDateTime will be overwritten with CreatedDateTime if it has no value"
    structured = source.select(
        from_json(Colname.timeseries, eventhub_timeseries_schema).alias("json")
    )
    flat = (
        structured.select(
            explode("json.Series"),
            col("json.Document.CreatedDateTime").alias("CreatedDateTime"),
        )
        .select(
            "col.MeteringPointId",
            "col.TransactionId",
            "col.RegistrationDateTime",
            "col.Period",
            "CreatedDateTime",
        )
        .select(
            col("MeteringPointId").alias(Colname.metering_point_id),
            col("TransactionId").alias(Colname.transaction_id),
            to_timestamp(col("CreatedDateTime")).alias("CreatedDateTime"),
            to_timestamp(col("RegistrationDateTime")).alias(
                Colname.registration_date_time
            ),
            to_timestamp(col("Period.StartDateTime")).alias("StartDateTime"),
            col("Period.Resolution").alias(Colname.resolution),
            explode("Period.Points").alias("Period_Point"),
        )
        .select(
            "*",
            col("Period_Point.Quantity").cast("decimal(18,3)").alias(Colname.quantity),
            col("Period_Point.Quality").alias(Colname.quality),
            "Period_Point.Position",
        )
        .drop("Period_Point")
    )

    flat = flat.withColumn(
        "TimeToAdd",
        when(
            col("Resolution") == Resolution.quarter, (col("Position") - 1) * 15
        ).otherwise(col("Position") - 1),
    )

    set_time_func = (
        when(
            col("Resolution") == Resolution.quarter,
            expr("StartDateTime + make_interval(0, 0, 0, 0, 0, TimeToAdd, 0)"),
        )
        .when(
            col("Resolution") == Resolution.hour,
            expr("StartDateTime + make_interval(0, 0, 0, 0, TimeToAdd, 0, 0)"),
        )
        .when(
            col("Resolution") == Resolution.day,
            expr("StartDateTime + make_interval(0, 0, 0, TimeToAdd, 0, 0, 0)"),
        )
        .when(
            col("Resolution") == Resolution.month,
            expr("StartDateTime + make_interval(0, TimeToAdd, 0, 0, 0, 0, 0)"),
        )
    )

    withTime = flat.withColumn(Colname.time, set_time_func).drop(
        "StartDateTime", "TimeToAdd"
    )

    withTime = (
        withTime.withColumn(Colname.year, year(col(Colname.time)))
        .withColumn(Colname.month, month(col(Colname.time)))
        .withColumn(Colname.day, dayofmonth(col(Colname.time)))
        .withColumn(
            Colname.registration_date_time,
            when(
                col(Colname.registration_date_time).isNull(), col("CreatedDateTime")
            ).otherwise(col(Colname.registration_date_time)),
        )
        .select(
            Colname.metering_point_id,
            Colname.transaction_id,
            Colname.quantity,
            Colname.quality,
            Colname.time,
            Colname.resolution,
            Colname.year,
            Colname.month,
            Colname.day,
            Colname.registration_date_time,
        )
    )

    return withTime
