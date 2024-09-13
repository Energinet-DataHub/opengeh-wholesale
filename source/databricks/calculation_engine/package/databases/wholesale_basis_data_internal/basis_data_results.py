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
from dependency_injector.wiring import inject, Provide

from package.calculation.calculation_output import BasisDataOutput
from package.constants import Colname
from package.container import Container
from package.infrastructure import logging_configuration
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from package.infrastructure.paths import (
    HiveBasisDataDatabase,
    WholesaleBasisDataInternalDatabase,
)


@logging_configuration.use_span("calculation.write.basis_data")
def write_basis_data(
    basis_data_output: BasisDataOutput,
) -> None:
    _write_basis_data(basis_data_output)


@inject
def _write_basis_data(
    basis_data_output: BasisDataOutput,
    infrastructure_settings: InfrastructureSettings = Provide[
        Container.infrastructure_settings
    ],
) -> None:
    with logging_configuration.start_span("metering_point_periods"):
        basis_data_output.metering_point_periods.withColumnRenamed(  # ToDO JMG: rename to "balance_responsible_party_id" earlier (TableReader?)
            Colname.balance_responsible_party_id, Colname.balance_responsible_party_id
        ).write.format(
            "delta"
        ).mode(
            "append"
        ).option(
            "mergeSchema", "false"
        ).insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.METERING_POINT_PERIODS_TABLE_NAME}"
        )

    with logging_configuration.start_span("time_series"):
        basis_data_output.time_series_points.write.format("delta").mode(
            "append"
        ).option("mergeSchema", "false").insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.TIME_SERIES_POINTS_TABLE_NAME}"
        )

        # ToDo JMG: Remove when we are on Unity Catalog
        basis_data_output.time_series_points.write.format("delta").mode(
            "append"
        ).option("mergeSchema", "false").insertInto(
            f"{HiveBasisDataDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.TIME_SERIES_POINTS_TABLE_NAME}"
        )

    with logging_configuration.start_span("grid_loss_metering_points"):
        basis_data_output.grid_loss_metering_points.write.format("delta").mode(
            "append"
        ).option("mergeSchema", "false").insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.GRID_LOSS_METERING_POINTS_TABLE_NAME}"
        )

    if basis_data_output.charge_price_information_periods:
        with logging_configuration.start_span("charge_price_information_periods"):
            basis_data_output.charge_price_information_periods.write.format(
                "delta"
            ).mode("append").option("mergeSchema", "false").insertInto(
                f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME}"
            )

    if basis_data_output.charge_price_points:
        with logging_configuration.start_span("charge_price_points"):
            basis_data_output.charge_price_points.write.format("delta").mode(
                "append"
            ).option("mergeSchema", "false").insertInto(
                f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.CHARGE_PRICE_POINTS_TABLE_NAME}"
            )

    if basis_data_output.charge_link_periods:
        with logging_configuration.start_span("charge_link_periods"):
            basis_data_output.charge_link_periods.write.format("delta").mode(
                "append"
            ).option("mergeSchema", "false").insertInto(
                f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.CHARGE_LINK_PERIODS_TABLE_NAME}"
            )
