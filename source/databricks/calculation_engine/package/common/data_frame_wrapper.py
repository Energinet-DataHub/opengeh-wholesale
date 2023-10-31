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
import pyspark.sql.types as t
from pyspark.sql import DataFrame

from package.common import assert_schema


class DataFrameWrapper:
    """
    Base class of "typed" data frames.
    The raw pyspark DataFrame is accessible as `data.df`.

    The wrapper will transform and verify the provided data frame:
    - Missing nullable columns will be added
    - Columns not present in the schema is removed
    - The resulting data frame is validated against the schema regarding types and
      nullability
    """

    def __init__(
        self,
        df: DataFrame,
        schema: t.StructType,
        ignore_nullability=False,
        ignore_decimal_scale=False,
        ignore_decimal_precision=False,
    ):
        df = DataFrameWrapper._add_missing_nullable_columns(df, schema)

        columns = [field.name for field in schema.fields]
        df = df.select(columns)

        assert_schema(
            df.schema,
            schema,
            ignore_nullability=ignore_nullability,
            ignore_column_order=True,
            ignore_decimal_scale=ignore_decimal_scale,
            ignore_decimal_precision=ignore_decimal_precision,
        )

        self._df: DataFrame = df

    @property
    def df(self) -> DataFrame:
        return self._df

    @staticmethod
    def _add_missing_nullable_columns(df: DataFrame, schema: t.StructType) -> DataFrame:
        """
        Utility method to add nullable fields that are expected by the schema,
        but are not present in the actual data frame.
        """
        for expected_field in schema:
            if expected_field.nullable and all(
                actual_field.name != expected_field.name
                for actual_field in df.schema.fields
            ):
                df = df.withColumn(
                    expected_field.name, f.lit(None).cast(expected_field.dataType)
                )

        return df
