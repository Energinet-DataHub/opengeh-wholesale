from dependency_injector.wiring import Provide, inject
from geh_common.telemetry import logging_configuration, use_span

from geh_wholesale.calculation.calculation_output import BasisDataOutput
from geh_wholesale.container import Container
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings
from geh_wholesale.infrastructure.paths import (
    WholesaleBasisDataInternalDatabase,
)


@use_span("calculation.write.basis_data")
def write_basis_data(
    basis_data_output: BasisDataOutput,
) -> None:
    _write_basis_data(basis_data_output)


@inject
def _write_basis_data(
    basis_data_output: BasisDataOutput,
    infrastructure_settings: InfrastructureSettings = Provide[Container.infrastructure_settings],
) -> None:
    with logging_configuration.start_span("metering_point_periods"):
        basis_data_output.metering_point_periods.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.METERING_POINT_PERIODS_TABLE_NAME}"
        )

    with logging_configuration.start_span("time_series"):
        basis_data_output.time_series_points.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.TIME_SERIES_POINTS_TABLE_NAME}"
        )

    with logging_configuration.start_span("grid_loss_metering_point_ids"):
        basis_data_output.grid_loss_metering_points.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.GRID_LOSS_METERING_POINT_IDS_TABLE_NAME}"
        )

    if basis_data_output.charge_price_information_periods:
        with logging_configuration.start_span("charge_price_information_periods"):
            basis_data_output.charge_price_information_periods.write.format("delta").mode("append").option(
                "mergeSchema", "false"
            ).insertInto(
                f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME}"
            )

    if basis_data_output.charge_price_points:
        with logging_configuration.start_span("charge_price_points"):
            basis_data_output.charge_price_points.write.format("delta").mode("append").option(
                "mergeSchema", "false"
            ).insertInto(
                f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.CHARGE_PRICE_POINTS_TABLE_NAME}"
            )

    if basis_data_output.charge_link_periods:
        with logging_configuration.start_span("charge_link_periods"):
            basis_data_output.charge_link_periods.write.format("delta").mode("append").option(
                "mergeSchema", "false"
            ).insertInto(
                f"{infrastructure_settings.catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{WholesaleBasisDataInternalDatabase.CHARGE_LINK_PERIODS_TABLE_NAME}"
            )
