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
from package.calculation.wholesale.data_structures import MonthlyAmountPerCharge
from package.constants import Colname, MonthlyAmountsColumnNames


def create(args: CalculatorArgs, monthly_amounts: MonthlyAmountPerCharge) -> DataFrame:
    monthly_amounts = add_metadata(
        args, _get_column_group_for_calculation_result_id(), monthly_amounts.df
    )
    monthly_amounts = _select_output_columns(monthly_amounts)

    return monthly_amounts


def _select_output_columns(df: DataFrame) -> DataFrame:
    # Map column names to the Delta table field names
    # Note: The order of the columns must match the order of the columns in the Delta table
    return df.select(
        col(Colname.calculation_id).alias(MonthlyAmountsColumnNames.calculation_id),
        col(Colname.calculation_type).alias(MonthlyAmountsColumnNames.calculation_type),
        col(Colname.calculation_execution_time_start).alias(
            MonthlyAmountsColumnNames.calculation_execution_time_start
        ),
        col(MonthlyAmountsColumnNames.calculation_result_id),
        col(Colname.grid_area_code).alias(MonthlyAmountsColumnNames.grid_area_code),
        col(Colname.energy_supplier_id).alias(
            MonthlyAmountsColumnNames.energy_supplier_id
        ),
        col(Colname.unit).alias(MonthlyAmountsColumnNames.quantity_unit),
        col(Colname.charge_time).alias(MonthlyAmountsColumnNames.time),
        col(Colname.total_amount).alias(MonthlyAmountsColumnNames.amount),
        col(Colname.charge_tax).alias(MonthlyAmountsColumnNames.is_tax),
        col(Colname.charge_code).alias(MonthlyAmountsColumnNames.charge_code),
        col(Colname.charge_type).alias(MonthlyAmountsColumnNames.charge_type),
        col(Colname.charge_owner).alias(MonthlyAmountsColumnNames.charge_owner_id),
    )


def _get_column_group_for_calculation_result_id() -> list[str]:
    return [
        Colname.calculation_id,
        Colname.charge_owner,
        Colname.grid_area_code,
        Colname.energy_supplier_id,
    ]
