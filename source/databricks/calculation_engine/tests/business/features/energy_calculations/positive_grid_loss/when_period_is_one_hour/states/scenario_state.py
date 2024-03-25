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

from pyspark.sql import DataFrame, SparkSession

import business.utils as cl
import business.utils.factories as clf
from package.codelists import (
    QuantityQuality,
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointResolution,
)


def get_expected(*args) -> DataFrame:
    """
    This function can be used to custom build the expected results (dataframe).
    It is also used a reference to locate the test scenario.
    """
    return cl.create_energy_result_dataframe(*args)


def create_grid_loss_metering_points(spark: SparkSession) -> DataFrame:
    factory = clf.InputGridLossTestFactory(spark)
    metering_point1 = factory.create_row(metering_point_id="571313180400100657")
    metering_point2 = factory.create_row(metering_point_id="571313180480500149")
    return factory.create_dataframe([metering_point1, metering_point2])


def create_time_series_points(spark: SparkSession) -> DataFrame:
    factory = clf.InputTimeSeriesPointTestFactory(spark)
    exchange_time_series = factory.create_row(
        metering_point_id="571313180400010437",
        quantity=10.0,
        quality=QuantityQuality.MEASURED.value,
        observation_time=datetime(2023, 2, 1, 12, 0, 0),
    )
    production_time_series = factory.create_row(
        metering_point_id="571313180400010673",
        quantity=5.0,
        quality=QuantityQuality.MEASURED.value,
        observation_time=datetime(2023, 2, 1, 12, 0, 0),
    )
    consumption_time_series = factory.create_row(
        metering_point_id="571313180400140417",
        quantity=10.0,
        quality=QuantityQuality.MEASURED.value,
        observation_time=datetime(2023, 2, 1, 12, 0, 0),
    )
    return factory.create_dataframe(
        [exchange_time_series, production_time_series, consumption_time_series]
    )


def create_metering_point_periods(spark: SparkSession) -> DataFrame:
    factory = clf.InputMeteringPointPeriodsTestFactory(spark)
    grid_loss_period = factory.create_row(
        metering_point_id="571313180400100657",
        metering_point_type=InputMeteringPointType.CONSUMPTION,
        settlement_method=InputSettlementMethod.FLEX,
        grid_area="804",
        resolution=MeteringPointResolution.QUARTER,
        energy_supplier="8100000000115",
        balance_responsible_id="5790001270940",
        from_date=datetime(2023, 1, 31, 23, 0, 0),
    )

    system_correction_period = factory.create_row(
        metering_point_id="571313180480500149",
        metering_point_type=InputMeteringPointType.PRODUCTION,
        grid_area="804",
        resolution=MeteringPointResolution.QUARTER,
        energy_supplier="8100000000108",
        balance_responsible_id="8100000000207",
        from_date=datetime(2023, 1, 31, 23, 0, 0),
    )

    exchange_period = factory.create_row(
        metering_point_id="571313180400140417",
        metering_point_type=InputMeteringPointType.EXCHANGE,
        grid_area="804",
        from_grid_area="803",
        to_grid_area="804",
        balance_responsible_id="8100000000207",
        from_date=datetime(2016, 12, 31, 23, 0, 0),
    )

    production_period = factory.create_row(
        metering_point_id="571313180400010437",
        metering_point_type=InputMeteringPointType.PRODUCTION,
        grid_area="804",
        energy_supplier="5790001687137",
        balance_responsible_id="5790000701414",
        from_date=datetime(2016, 12, 31, 23, 0, 0),
    )

    consumption_period = factory.create_row(
        metering_point_id="571313180400010673",
        metering_point_type=InputMeteringPointType.CONSUMPTION,
        grid_area="804",
        energy_supplier="5790001687137",
        balance_responsible_id="5790000701414",
        from_date=datetime(2016, 12, 31, 23, 0, 0),
    )

    return factory.create_dataframe(
        [
            grid_loss_period,
            system_correction_period,
            exchange_period,
            production_period,
            consumption_period,
        ]
    )
