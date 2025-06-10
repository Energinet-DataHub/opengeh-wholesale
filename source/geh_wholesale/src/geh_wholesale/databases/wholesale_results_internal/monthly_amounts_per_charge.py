from dependency_injector.wiring import Provide, inject
from geh_common.telemetry import logging_configuration, use_span
from pyspark.sql import DataFrame

from geh_wholesale.calculation.calculation_output import WholesaleResultsOutput
from geh_wholesale.container import Container
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings
from geh_wholesale.infrastructure.paths import (
    WholesaleResultsInternalDatabase,
)


@use_span("calculation.write.wholesale")
def write_monthly_amounts_per_charge(
    wholesale_results_output: WholesaleResultsOutput,
) -> None:
    """Write each wholesale result to the output table."""
    _write(
        "monthly_tariff_from_hourly_per_co_es",
        wholesale_results_output.monthly_tariff_from_hourly_per_co_es,
    )
    _write(
        "monthly_tariff_from_daily_per_co_es",
        wholesale_results_output.monthly_tariff_from_daily_per_co_es,
    )
    _write(
        "monthly_subscription_per_co_es",
        wholesale_results_output.monthly_subscription_per_co_es,
    )
    _write(
        "monthly_fee_per_co_es",
        wholesale_results_output.monthly_fee_per_co_es,
    )


@inject
def _write(
    name: str,
    df: DataFrame,
    infrastructure_settings: InfrastructureSettings = Provide[Container.infrastructure_settings],
) -> None:
    with logging_configuration.start_span(name):
        df.write.format("delta").mode("append").option("mergeSchema", "false").insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME}"
        )
