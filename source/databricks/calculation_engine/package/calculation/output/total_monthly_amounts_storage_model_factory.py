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
from package.calculation.wholesale.data_structures import TotalMonthlyAmount
from package.constants import Colname, WholesaleResultColumnNames


def create(
    args: CalculatorArgs, total_monthly_amounts: TotalMonthlyAmount
) -> DataFrame:
    total_monthly_amounts = _add_metadata(args, total_monthly_amounts.df)
    total_monthly_amounts = _add_calculation_result_id(total_monthly_amounts)
    total_monthly_amounts = _select_output_columns(total_monthly_amounts)

    return total_monthly_amounts


def _add_metadata(args: CalculatorArgs, df: DataFrame) -> DataFrame:
    return (
        df.withColumn(Colname.calculation_id, lit(args.calculation_id))
        .withColumn(Colname.calculation_type, lit(args.calculation_type.value))
        .withColumn(
            Colname.calculation_execution_time_start,
            lit(args.calculation_execution_time_start),
        )
    )


def _add_calculation_result_id(df: DataFrame) -> DataFrame:
    df = df.withColumn(
        WholesaleResultColumnNames.calculation_result_id, f.expr("uuid()")
    )
    window = Window.partitionBy(_get_column_group_for_calculation_result_id())
    return df.withColumn(
        WholesaleResultColumnNames.calculation_result_id,
        first(col(WholesaleResultColumnNames.calculation_result_id)).over(window),
    )


def _select_output_columns(df: DataFrame) -> DataFrame:
    # Map column names to the Delta table field names
    # Note: The order of the columns must match the order of the columns in the Delta table
    return df.select(
        col(Colname.calculation_id).alias(WholesaleResultColumnNames.calculation_id),
        col(Colname.calculation_type).alias(
            WholesaleResultColumnNames.calculation_type
        ),
        col(Colname.calculation_execution_time_start).alias(
            WholesaleResultColumnNames.calculation_execution_time_start
        ),
        col(WholesaleResultColumnNames.calculation_result_id),
        col(Colname.grid_area).alias(WholesaleResultColumnNames.grid_area),
        col(Colname.energy_supplier_id).alias(
            WholesaleResultColumnNames.energy_supplier_id
        ),
        col(Colname.charge_time).alias(WholesaleResultColumnNames.time),
        col(Colname.total_amount).alias(WholesaleResultColumnNames.amount),
        col(Colname.charge_owner).alias(WholesaleResultColumnNames.charge_owner_id),
    )


def _get_column_group_for_calculation_result_id() -> list[str]:
    return [
        Colname.calculation_id,
        Colname.charge_owner,
        Colname.grid_area,
        Colname.energy_supplier_id,
    ]
