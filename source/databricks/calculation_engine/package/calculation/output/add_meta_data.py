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
from pyspark.sql.functions import col, lit, first
from pyspark.sql.window import Window

from package.calculation.calculator_args import CalculatorArgs
from package.constants import Colname
from package.constants.result_column_names import ResultColumnNames


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


def _add_calculation_result_id(
    df: DataFrame, column_group_for_calculation_result_id: list[str]
) -> DataFrame:
    df = df.withColumn(ResultColumnNames.calculation_result_id, f.expr("uuid()"))
    window = Window.partitionBy(column_group_for_calculation_result_id)
    return df.withColumn(
        ResultColumnNames.calculation_result_id,
        first(col(ResultColumnNames.calculation_result_id)).over(window),
    )
