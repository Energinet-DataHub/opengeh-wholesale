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
import pyspark.sql.functions as f
from pyspark.sql import DataFrame
from pyspark.sql.types import (
    BooleanType,
    IntegerType,
)
from pyspark.sql.types import (
    StructField,
    StringType,
    TimestampType,
    StructType,
    ArrayType,
    DecimalType,
    LongType,
)

from package.databases.table_column_names import TableColumnNames


def cast_column_types(df: DataFrame, table_or_view_name: str = "") -> DataFrame:
    """
    Cast the columns of a DataFrame to the correct types.
    """
    for column in df.schema:
        df = _cast_column(df, column.name, table_or_view_name)
    return df


def _cast_column(df: DataFrame, column_name: str, table_or_view_name: str) -> DataFrame:

    # Needed to avoid "time_series_type" will be cast to a timestamp.
    if column_name == "time_series_type":
        return df

    if (
        "time" in column_name
        or "period" in column_name
        or "date" in column_name
        or "day" in column_name
    ):
        return df.withColumn(column_name, f.col(column_name).cast(TimestampType()))

    if column_name.endswith("version"):
        return df.withColumn(column_name, f.col(column_name).cast(LongType()))

    if column_name == "quantity":
        if "charge_link" in table_or_view_name:
            return df.withColumn(column_name, f.col(column_name).cast(IntegerType()))
        else:
            return df.withColumn(
                column_name, f.col(column_name).cast(DecimalType(18, 3))
            )
    if column_name == "charge_link_quantity":
        return df.withColumn(column_name, f.col(column_name).cast(IntegerType()))

    if column_name == "quantities":
        """Settlement report quantities are stored as a string in the format "[{observation_time: timestamp, quantity: decimal}, ...]"."""
        return df.withColumn(
            column_name,
            f.from_json(
                f.col(column_name), ArrayType(_settlement_report_quantity_schema)
            ),
        )

    if "qualities" in column_name:
        return df.withColumn(
            column_name,
            f.split(
                f.regexp_replace(
                    f.regexp_replace(f.col(column_name), r"[\[\]']", ""),
                    " ",
                    "",
                ),
                ",",
            ).cast(ArrayType(StringType())),
        )

    if column_name == "price_points":
        return df.withColumn(
            column_name,
            f.from_json(f.col(column_name), ArrayType(_price_point)),
        )

    if "price" in column_name or column_name == "amount":
        return df.withColumn(column_name, f.col(column_name).cast(DecimalType(18, 6)))

    if column_name == "is_tax":
        return df.withColumn(column_name, f.col(column_name).cast(BooleanType()))

    if column_name == "is_internal_calculation":
        return df.withColumn(column_name, f.col(column_name).cast(BooleanType()))

    return df


# TODO BJM: Replace these by recursive type casting
_settlement_report_quantity_schema = StructType(
    [
        StructField(TableColumnNames.observation_time, TimestampType(), False),
        StructField(TableColumnNames.quantity, DecimalType(18, 3), False),
    ]
)


_price_point = StructType(
    [
        StructField("time", TimestampType(), True),
        StructField("price", DecimalType(18, 6), True),
    ]
)
