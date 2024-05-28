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
from package.calculation.calculation_results import BasisDataContainer
from package.infrastructure import logging_configuration
from package.infrastructure.paths import (
    METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
    TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME,
    BASIS_DATA_DATABASE_NAME,
    CHARGE_MASTER_DATA_PERIODS_BASIS_DATA_TABLE_NAME,
    CHARGE_PRICE_POINTS_BASIS_DATA_TABLE_NAME,
    CHARGE_LINK_PERIODS_BASIS_DATA_TABLE_NAME,
    GRID_LOSS_METERING_POINTS_BASIS_DATA_TABLE_NAME,
)


@logging_configuration.use_span("calculation.write.basis_data")
def write_basis_data(basis_data: BasisDataContainer) -> None:
    with logging_configuration.start_span("metering_point_periods"):
        basis_data.metering_point_periods.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{BASIS_DATA_DATABASE_NAME}.{METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME}"
        )

    with logging_configuration.start_span("time_series"):
        basis_data.time_series_points.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{BASIS_DATA_DATABASE_NAME}.{TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME}"
        )

    with logging_configuration.start_span("grid_loss_metering_points"):
        basis_data.grid_loss_metering_points.write.format("delta").mode(
            "append"
        ).option("mergeSchema", "false").insertInto(
            f"{BASIS_DATA_DATABASE_NAME}.{GRID_LOSS_METERING_POINTS_BASIS_DATA_TABLE_NAME}"
        )

    if basis_data.charge_price_information_periods:
        with logging_configuration.start_span("charge_master_data"):
            basis_data.charge_price_information_periods.write.format("delta").mode(
                "append"
            ).option("mergeSchema", "false").insertInto(
                f"{BASIS_DATA_DATABASE_NAME}.{CHARGE_MASTER_DATA_PERIODS_BASIS_DATA_TABLE_NAME}"
            )

    if basis_data.charge_prices:
        with logging_configuration.start_span("charge_prices"):
            basis_data.charge_prices.write.format("delta").mode("append").option(
                "mergeSchema", "false"
            ).insertInto(
                f"{BASIS_DATA_DATABASE_NAME}.{CHARGE_PRICE_POINTS_BASIS_DATA_TABLE_NAME}"
            )

    if basis_data.charge_links:
        with logging_configuration.start_span("charge_links"):
            basis_data.charge_links.write.format("delta").mode("append").option(
                "mergeSchema", "false"
            ).insertInto(
                f"{BASIS_DATA_DATABASE_NAME}.{CHARGE_LINK_PERIODS_BASIS_DATA_TABLE_NAME}"
            )
