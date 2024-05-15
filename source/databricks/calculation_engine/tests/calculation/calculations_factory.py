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
from datetime import datetime

from pyspark.sql import DataFrame, Row, SparkSession

from package.calculation.basis_data.schemas import calculations_schema
from package.codelists import CalculationType
from package.constants.calculation_column_names import CalculationColumnNames


class DefaultValues:
    CALCULATION_ID = "ff3b3b3b-3b3b-3b3b-3b3b-3b3b3b3b3b3b"
    CALCULATION_TYPE = CalculationType.BALANCE_FIXING
    CALCULATION_PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)
    CALCULATION_PERIOD_END_DATETIME = datetime(2020, 1, 31, 23)
    CALCULATION_EXECUTION_TIME_START = datetime(2020, 2, 1, 11)
    CREATED_BY_USER_ID = "aa3b3b3b-3b3b-3b3b-3b3b-3b3b3b3b3b3b"
    VERSION = 7


def create_calculation(
    calculation_id: str = DefaultValues.CALCULATION_ID,
    calculation_type: CalculationType = DefaultValues.CALCULATION_TYPE,
    calculation_period_start_datetime: datetime = DefaultValues.CALCULATION_PERIOD_START_DATETIME,
    calculation_period_end_datetime: datetime = DefaultValues.CALCULATION_PERIOD_END_DATETIME,
    calculation_execution_time_start: datetime = DefaultValues.CALCULATION_EXECUTION_TIME_START,
    created_by_user_id: str = DefaultValues.CREATED_BY_USER_ID,
    version: int = DefaultValues.VERSION,
) -> Row:
    calculation = {
        CalculationColumnNames.calculation_id: calculation_id,
        CalculationColumnNames.calculation_type: calculation_type.value,
        CalculationColumnNames.period_start: calculation_period_start_datetime,
        CalculationColumnNames.period_end: calculation_period_end_datetime,
        CalculationColumnNames.execution_time_start: calculation_execution_time_start,
        CalculationColumnNames.created_by_user_id: created_by_user_id,
        CalculationColumnNames.version: version,
    }

    return Row(**calculation)


def create_calculations(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> DataFrame:
    if data is None:
        data = [create_calculation()]
    elif isinstance(data, Row):
        data = [data]
    return spark.createDataFrame(data=data, schema=calculations_schema)


def create_empty_calculations(spark: SparkSession) -> DataFrame:
    return spark.createDataFrame(data=[], schema=calculations_schema)
