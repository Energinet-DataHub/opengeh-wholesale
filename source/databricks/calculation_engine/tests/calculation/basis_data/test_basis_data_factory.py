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
from package.calculation.calculator_args import CalculatorArgs
from tests.calculation.preparation.transformations import metering_point_periods_factory
from tests.calculation.preparation.transformations import (
    prepared_metering_point_time_series_factory,
)
from package.calculation.calculation_results import BasisDataContainer
import package.calculation.basis_data.basis_data_factory as basis_data_factory
from package.calculation.preparation.data_structures import InputChargesContainer
from package.calculation.preparation.data_structures.charge_master_data import (
    ChargeMasterData,
)
import tests.calculation.charges_factory as charges_factory
from package.calculation.preparation.data_structures.charge_prices import ChargePrices
from package.calculation.basis_data.schemas.charge_link_periods_schema import (
    charge_link_periods_schema,
)
from package.calculation.basis_data.schemas.charge_master_data_periods_schema import (
    charge_master_data_periods_schema,
)
from package.calculation.basis_data.schemas.charge_price_points_schema import (
    charge_price_points_schema,
)
from package.calculation.basis_data.schemas.time_series_point_schema import (
    time_series_point_schema,
)
from package.calculation.basis_data.schemas.metering_point_period_schema import (
    metering_point_period_schema,
)
from datetime import timedelta, datetime
from decimal import Decimal
import package.calculation.preparation.data_structures as d
import package.codelists as e
from package.codelists import ChargeType
from package.constants import Colname
import pytest
from pyspark.sql import Row, SparkSession, DataFrame


class DefaultValues:
    CALCULATION_ID = "12345"
    GRID_AREA = "543"
    CHARGE_TYPE = ChargeType.TARIFF
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    CHARGE_TAX = True
    CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23)
    CHARGE_PRICE = Decimal("2.000005")
    CHARGE_QUANTITY = 1
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
    SETTLEMENT_METHOD = e.SettlementMethod.FLEX
    QUANTITY = Decimal("1.005")
    PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)
    FROM_DATE: datetime = datetime(2019, 12, 31, 23)
    TO_DATE: datetime = datetime(2020, 1, 31, 23)
    TIME_ZONE = "Europe/Copenhagen"
    CALCULATION_GRID_AREAS = ["805", "806"]
    CALCULATION_PERIOD_START_DATETIME=datetime(2018, 1, 1, 23, 0, 0)
    CALCULATION_PERIOD_END_DATETIME=datetime(2018, 1, 3, 23, 0, 0)
    CALCULATION_EXECUTION_TIME_START=datetime(2018, 1, 5, 23, 0, 0)
    QUARTERLY_RESOLUTION_TRANSITION_DATETIME=datetime(2018, 1, 5, 23, 0, 0)
    CREATED_BY_USER_ID="bar"


def _create_charge_master_data_row(
    calculation_id: str = DefaultValues.CALCULATION_ID,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_type: ChargeType = DefaultValues.CHARGE_TYPE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    charge_tax: bool = DefaultValues.CHARGE_TAX,
    resolution: e.ChargeResolution = e.ChargeResolution.HOUR,
    from_date: datetime = DefaultValues.FROM_DATE,
    to_date: datetime | None = DefaultValues.TO_DATE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.calculation_id: calculation_id,
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.resolution: resolution.value,
        Colname.charge_tax: charge_tax,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }

    return Row(**row)


def _create_charge_prices_row(
    calculation_id: str = DefaultValues.CALCULATION_ID,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_type: ChargeType = DefaultValues.CHARGE_TYPE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    charge_time: datetime = DefaultValues.CHARGE_TIME_HOUR_0,
    charge_price: Decimal = DefaultValues.CHARGE_PRICE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.calculation_id: calculation_id,
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_price: charge_price,
        Colname.charge_time: charge_time,
    }

    return Row(**row)


def _create_charge_link_row(
    calculation_id: str = DefaultValues.CALCULATION_ID,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_type: ChargeType = DefaultValues.CHARGE_TYPE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    charge_time: datetime = DefaultValues.CHARGE_TIME_HOUR_0,
    charge_price: Decimal = DefaultValues.CHARGE_PRICE,
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    quantity: int = DefaultValues.CHARGE_QUANTITY,
    from_date: datetime = DefaultValues.FROM_DATE,
    to_date: datetime | None = DefaultValues.TO_DATE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.calculation_id: calculation_id,
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: quantity,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }

    return Row(**row)


def _create_charge_master_data(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> ChargeMasterData:
    if data is None:
        data = [_create_charge_master_data_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, charge_master_data_periods_schema)
    return ChargeMasterData(df)


def _create_charge_prices(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> ChargePrices:
    if data is None:
        data = [_create_charge_prices_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, charge_price_points_schema)
    return ChargePrices(df)


def _create_charge_links(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> DataFrame:
    if data is None:
        data = [_create_charge_link_row()]
    elif isinstance(data, Row):
        data = [data]
    return spark.createDataFrame(data, charge_link_periods_schema)


def _create_calculation_args() -> CalculatorArgs:
    return CalculatorArgs(
        calculation_id=DefaultValues.CALCULATION_ID,
        calculation_type=e.CalculationType.AGGREGATION,
        calculation_grid_areas=DefaultValues.CALCULATION_GRID_AREAS,
        calculation_period_start_datetime=DefaultValues.CALCULATION_PERIOD_START_DATETIME,
        calculation_period_end_datetime=DefaultValues.CALCULATION_PERIOD_END_DATETIME,
        calculation_execution_time_start=DefaultValues.CALCULATION_EXECUTION_TIME_START,
        time_zone=DefaultValues.TIME_ZONE,
        quarterly_resolution_transition_datetime=DefaultValues.QUARTERLY_RESOLUTION_TRANSITION_DATETIME,
        created_by_user_id=DefaultValues.CREATED_BY_USER_ID,
    )


def _create_prepared_metering_point_time_series(spark: SparkSession):
    time_series_rows = [
        charges_factory.create_time_series_row(),
        charges_factory.create_time_series_row(),
    ]

    metering_point_time_series_df = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )

    return metering_point_time_series_df


def _create_basis_data_factory(spark: SparkSession) -> BasisDataContainer:
    calculation_args = _create_calculation_args()
    metering_point_period_df = metering_point_periods_factory.create(spark)
    metering_point_time_series_df = _create_prepared_metering_point_time_series(spark)
    charge_links = _create_charge_links(spark)
    charge_prices = _create_charge_prices(spark)
    charge_master_data = _create_charge_master_data(spark)

    input_charges_container = InputChargesContainer(
        charge_master_data=charge_master_data,
        charge_prices=charge_prices,
        charge_links=charge_links,
    )

    return basis_data_factory.create(
        args=calculation_args,
        metering_point_periods_df=metering_point_period_df,
        metering_point_time_series_df=metering_point_time_series_df,
        input_charges_container=input_charges_container,
    )


def test__basis_data_uses_correct_schema(spark: SparkSession):
    basis_data_container = _create_basis_data_factory(spark)
    assert (
        basis_data_container.metering_point_periods.schema
        == metering_point_period_schema
    )
    assert basis_data_container.time_series_points.schema == time_series_point_schema
    assert (
        basis_data_container.charge_master_data.schema
        == charge_master_data_periods_schema
    )
    assert basis_data_container.charge_prices.schema == charge_price_points_schema
    assert basis_data_container.charge_links.schema == charge_link_periods_schema
