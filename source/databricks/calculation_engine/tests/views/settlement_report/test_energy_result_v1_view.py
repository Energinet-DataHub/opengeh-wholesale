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
import pathlib

from pyspark.sql import SparkSession, dataframe

from features.utils.factories.settlement_report.settlement_report_energy_results_view_test_factory import (
    SettlementReportEnergyResultsViewTestFactory,
)
from helpers.data_frame_utils import assert_dataframes_equal
from package.constants import MeteringPointPeriodColname
from package.infrastructure.paths import (
    SETTLEMENT_REPORT_DATABASE_NAME,
    ENERGY_RESULTS_SETTLEMENT_REPORT_VIEW_NAME_V1,
    OUTPUT_DATABASE_NAME,
    ENERGY_RESULT_TABLE_NAME,
)


def create_expected(spark: SparkSession, df: dataframe) -> dataframe:
    view_factory = SettlementReportEnergyResultsViewTestFactory(spark)
    first = df.first()

    row = view_factory.create_row(
        first[MeteringPointPeriodColname.calculation_id],
        first[MeteringPointPeriodColname.metering_point_id],
        first[MeteringPointPeriodColname.from_date],
        first[MeteringPointPeriodColname.to_date],
        first[MeteringPointPeriodColname.grid_area],
        first[MeteringPointPeriodColname.from_grid_area],
        first[MeteringPointPeriodColname.to_grid_area],
        first[MeteringPointPeriodColname.metering_point_type],
        first[MeteringPointPeriodColname.settlement_method],
        first[MeteringPointPeriodColname.energy_supplier_id],
    )

    return view_factory.create_dataframe([row])


def test__read_energy_results__returns_expected(
    spark: SparkSession,
    migrations_executed: None,
    tmp_path: pathlib.Path,
) -> None:
    """
    The test verifies that the view "energy_results_v1" is updated when the underlying
    basis_data.metering_point_periods table is updated (and that the view exists in the
    wholesale schema (database) settlement_report).
    """
    # Arrange
    factory = EnergyResultsFactory(spark)
    row = factory.create_row()
    df = factory.create_dataframe([row])
    df.write.format("delta").mode("overwrite").saveAsTable(
        f"{OUTPUT_DATABASE_NAME}.{ENERGY_RESULT_TABLE_NAME}"
    )
    expected = create_expected(spark, df)

    # Act
    actual = spark.read.format("delta").table(
        f"{SETTLEMENT_REPORT_DATABASE_NAME}.{ENERGY_RESULTS_SETTLEMENT_REPORT_VIEW_NAME_V1}"
    )

    # Assert
    assert_dataframes_equal(actual, expected)
