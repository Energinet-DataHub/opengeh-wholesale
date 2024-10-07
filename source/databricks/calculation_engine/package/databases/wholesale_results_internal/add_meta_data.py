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
from pyspark.sql.functions import col, lit
from pyspark.sql.window import Window

from package.calculation.calculator_args import CalculatorArgs
from package.constants import Colname
from package.databases.table_column_names import TableColumnNames


def add_metadata(
    args: CalculatorArgs,
    column_group_for_calculation_result_id: list[str],
    df: DataFrame,
) -> DataFrame:
    df = (
        df.withColumn(Colname.calculation_id, lit(args.calculation_id))
        .withColumn(Colname.calculation_type, lit(args.calculation_type.value))
        .withColumn(
            Colname.calculation_execution_time_start,
            lit(args.calculation_execution_time_start),
        )
    )
    df = _add_calculation_result_id(df, column_group_for_calculation_result_id)
    return df


# def _add_calculation_result_id(
#     df: DataFrame, column_group_for_calculation_result_id: list[str]
# ) -> DataFrame:
#
#     # TODO AJW: Rename to result_id when we are on Unity Catalog.
#     df = df.withColumn(TableColumnNames.calculation_result_id, f.expr("uuid()"))
#     window = Window.partitionBy(column_group_for_calculation_result_id)
#     return df.withColumn(
#         # TODO AJW: Rename to result_id when we are on Unity Catalog.
#         TableColumnNames.calculation_result_id,
#         # TODO AJW: Rename to result_id when we are on Unity Catalog.
#         first(col(TableColumnNames.calculation_result_id)).over(window),
#     )


def _add_calculation_result_id(
    df: DataFrame, column_group_for_calculation_result_id: list[str]
) -> DataFrame:
    return add_calculation_result_id(
        df,
        column_group_for_calculation_result_id,
        TableColumnNames.calculation_result_id,
    )


def add_calculation_result_id(
    df: DataFrame, column_group_for_calculation_result_id: list[str], result_id_col: str
) -> DataFrame:
    # Concatenate the values of the all columns in dataframe
    # Generate a deterministic hash based on partition columns

    df.show()

    b = f.concat_ws("", *[col(c) for c in df.columns])
    print(b)

    df = df.withColumn(
        result_id_col,
        f.sha2(f.concat_ws("", *[col(c) for c in df.columns]), 256),
    )

    df.show()

    window = Window.partitionBy(column_group_for_calculation_result_id)

    # Ensure the calculation result ID is consistent within each partition
    return df.withColumn(result_id_col, f.first(col(result_id_col)).over(window))
