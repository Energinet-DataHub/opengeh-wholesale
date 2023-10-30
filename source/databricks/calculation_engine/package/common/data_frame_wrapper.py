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

from dataclasses import dataclass

from pyspark.sql import DataFrame
import pyspark.sql.functions as f
import pyspark.sql.types as t


@dataclass(frozen=True)
class DataFrameWrapper:
    """
    Base class of "typed" data frames.
    The raw pyspark DataFrame is accessible as `data.df`.
    """

    df: DataFrame
    """
    The data frame.
    """

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
