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

from calculation_logic.features.energy_calculations.energy_results_dataframe import (
    create_energy_result_dataframe,
)
from calculation_logic.input_factories.input_grid_loss_test_factory import (
    InputGridLossTestFactory,
)
from calculation_logic.input_factories.input_metering_point_periods_test_factory import (
    InputMeteringPointPeriodsTestFactory,
)
from calculation_logic.input_factories.input_time_series_point_test_factory import (
    InputTimeSeriesPointTestFactory,
)
from package.codelists import (
    InputMeteringPointType,
    MeteringPointResolution,
    InputSettlementMethod,
    QuantityQuality,
)


def test_setup(*args) -> DataFrame:
    """
    This function can be used to custom build the expected results (dataframe).
    It is also used a reference to locate the test scenario.
    """
    return create_energy_result_dataframe(*args)


def create_grid_loss_metering_points(spark: SparkSession) -> DataFrame:
    factory = InputGridLossTestFactory(spark)
    row1 = factory.create_row("571313180400100657")
    row2 = factory.create_row("571313180480500149")
    return factory.create_dataframe([row1, row2])


def create_time_series_points(spark: SparkSession) -> DataFrame:
    factory = InputTimeSeriesPointTestFactory(spark)
    row1 = factory.create_row(
        "571313180400010437",
        10.0,
        QuantityQuality.MEASURED.value,
        datetime(2023, 2, 1, 12, 0, 0),
    )
    row2 = factory.create_row(
        "571313180400010673",
        5.0,
        QuantityQuality.MEASURED.value,
        datetime(2023, 2, 1, 12, 0, 0),
    )
    row3 = factory.create_row(
        "571313180400140417",
        10.0,
        QuantityQuality.MEASURED.value,
        datetime(2023, 2, 1, 12, 0, 0),
    )
    return factory.create_dataframe([row1, row2, row3])


def create_metering_point_periods(spark: SparkSession) -> DataFrame:
    factory = InputMeteringPointPeriodsTestFactory(spark)
    row1 = factory.create_row(
        metering_point_id="571313180400100657",
        metering_point_type=InputMeteringPointType.CONSUMPTION,
        settlement_method=InputSettlementMethod.FLEX,
        grid_area="804",
        resolution=MeteringPointResolution.QUARTER,
        energy_supplier="8100000000115",
        balance_responsible_id="5790001270940",
        from_date=datetime(2023, 1, 31, 23, 0, 0),
    )

    row2 = factory.create_row(
        metering_point_id="571313180480500149",
        metering_point_type=InputMeteringPointType.PRODUCTION,
        grid_area="804",
        resolution=MeteringPointResolution.QUARTER,
        energy_supplier="8100000000108",
        balance_responsible_id="8100000000207",
        from_date=datetime(2023, 1, 31, 23, 0, 0),
    )

    row3 = factory.create_row(
        metering_point_id="571313180400140417",
        metering_point_type=InputMeteringPointType.EXCHANGE,
        grid_area="804",
        from_grid_area="803",
        to_grid_area="804",
        balance_responsible_id="8100000000207",
        from_date=datetime(2016, 12, 31, 23, 0, 0),
    )

    row4 = factory.create_row(
        metering_point_id="571313180400010437",
        metering_point_type=InputMeteringPointType.PRODUCTION,
        grid_area="804",
        energy_supplier="5790001687137",
        balance_responsible_id="5790000701414",
        from_date=datetime(2016, 12, 31, 23, 0, 0),
    )

    row5 = factory.create_row(
        metering_point_id="571313180400010673",
        metering_point_type=InputMeteringPointType.CONSUMPTION,
        grid_area="804",
        energy_supplier="5790001687137",
        balance_responsible_id="5790000701414",
        from_date=datetime(2016, 12, 31, 23, 0, 0),
    )

    return factory.create_dataframe(
        [
            row1,
            row2,
            row3,
            row4,
            row5,
        ]
    )
