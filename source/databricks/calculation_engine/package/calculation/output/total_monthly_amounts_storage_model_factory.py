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

from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from package.calculation.calculator_args import CalculatorArgs
from package.calculation.output.add_meta_data import add_metadata
from package.calculation.wholesale.data_structures import TotalMonthlyAmount
from package.constants import Colname, TotalMonthlyAmountsColumnNames


def create(
    args: CalculatorArgs, total_monthly_amounts: TotalMonthlyAmount
) -> DataFrame:
    total_monthly_amounts = add_metadata(
        args, _get_column_group_for_calculation_result_id(), total_monthly_amounts.df
    )
    total_monthly_amounts = _select_output_columns(total_monthly_amounts)

    return total_monthly_amounts


def _select_output_columns(df: DataFrame) -> DataFrame:
    # Map column names to the Delta table field names
    # Note: The order of the columns must match the order of the columns in the Delta table
    return df.select(
        col(Colname.calculation_id).alias(
            TotalMonthlyAmountsColumnNames.calculation_id
        ),
        col(TotalMonthlyAmountsColumnNames.calculation_result_id),
        col(Colname.grid_area).alias(TotalMonthlyAmountsColumnNames.grid_area),
        col(Colname.energy_supplier_id).alias(
            TotalMonthlyAmountsColumnNames.energy_supplier_id
        ),
        col(Colname.charge_time).alias(TotalMonthlyAmountsColumnNames.time),
        col(Colname.total_amount).alias(TotalMonthlyAmountsColumnNames.amount),
        col(Colname.charge_owner).alias(TotalMonthlyAmountsColumnNames.charge_owner_id),
    )


def _get_column_group_for_calculation_result_id() -> list[str]:
    return [
        Colname.calculation_id,
        Colname.charge_owner,
        Colname.grid_area,
        Colname.energy_supplier_id,
    ]
