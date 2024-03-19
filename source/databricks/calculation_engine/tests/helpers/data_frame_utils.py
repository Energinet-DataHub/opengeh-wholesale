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

from package.common import assert_schema


def set_column(
    df: DataFrame,
    column_name: str,
    column_value: str | list,
) -> DataFrame:
    """Set the column value of all rows in the data frame."""
    if isinstance(column_value, list):
        return df.withColumn(column_name, f.array(*map(f.lit, column_value)))
    return df.withColumn(column_name, f.lit(column_value))


def assert_dataframes_equal(actual: DataFrame, expected: DataFrame) -> None:
    assert actual.subtract(expected).count() == 0
    assert expected.subtract(actual).count() == 0


def assert_dataframe_and_schema(
    actual: DataFrame,
    expected: DataFrame,
    ignore_nullability: bool = False,
    ignore_column_order: bool = False,
    ignore_decimal_scale: bool = False,
    ignore_decimal_precision: bool = False,
    columns_to_skip: any = None,
    show_dataframe=False,
    show_schema=False,
) -> None:
    if columns_to_skip is not None:
        actual = actual.drop(*columns_to_skip)
        expected = expected.drop(*columns_to_skip)

    if show_schema:
        actual.printSchema()
        expected.printSchema()

    assert_schema(
        actual.schema,
        expected.schema,
        ignore_nullability,
        ignore_column_order,
        ignore_decimal_scale,
        ignore_decimal_precision,
    )

    if show_dataframe:
        print("ACTUAL:")
        actual.show(n=3000)
        print("EXPECTED:")
        expected.show(n=3000)

    assert actual.subtract(expected).count() == 0
