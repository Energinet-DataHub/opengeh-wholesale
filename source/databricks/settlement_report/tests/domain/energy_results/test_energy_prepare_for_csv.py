from datetime import datetime

import pytest
from pyspark.sql import SparkSession, DataFrame
import pyspark.sql.functions as F

import test_factories.default_test_data_spec as default_data
from settlement_report_job.domain.csv_column_names import CsvColumnNames
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.energy_results.prepare_for_csv import prepare_for_csv
from settlement_report_job.wholesale.data_values import (
    MeteringPointTypeDataProductValue,
    SettlementMethodDataProductValue,
)
import test_factories.energy_factory as energy_factory

DEFAULT_TIME_ZONE = "Europe/Copenhagen"


def _create_energy_with_many_combinations(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    per_energy_supplier: bool,
) -> DataFrame:
    df = None
    for grid_area_code in ["804", "805"]:
        for energy_supplier_id in ["1000000000000", "2000000000000"]:
            for mp_type, settlement_method in [
                (
                    MeteringPointTypeDataProductValue.CONSUMPTION,
                    SettlementMethodDataProductValue.FLEX,
                ),
                (
                    MeteringPointTypeDataProductValue.CONSUMPTION,
                    SettlementMethodDataProductValue.NON_PROFILED,
                ),
                (MeteringPointTypeDataProductValue.PRODUCTION, None),
            ]:

                spec = default_data.create_energy_results_data_spec(
                    from_date=from_date,
                    to_date=to_date,
                    grid_area_code=grid_area_code,
                    metering_point_type=mp_type,
                    settlement_method=(
                        settlement_method.value
                        if settlement_method is not None
                        else None
                    ),  # ToDo JMG: support enums in factory
                    energy_supplier_id=energy_supplier_id,
                )
                if per_energy_supplier:
                    current_df = energy_factory.create_energy_per_es_v1(spark, spec)
                else:
                    current_df = energy_factory.create_energy_v1(spark, spec)

                if df is None:
                    df = current_df
                else:
                    df = df.union(current_df)

    return df


@pytest.mark.parametrize(
    "requesting_actor_market_role",
    [
        MarketRole.ENERGY_SUPPLIER,
        MarketRole.GRID_ACCESS_PROVIDER,
    ],
)
def test_prepare_for_csv__when_energy_supplier_or_grid_access_provider__returns_dataframe_with_expected_ordering(
    spark: SparkSession,
    requesting_actor_market_role: MarketRole,
) -> None:
    # Arrange
    from_date = datetime(2023, 3, 10, 22)
    to_date = datetime(2023, 3, 12, 22)

    expected_order = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.time,
    ]

    df = _create_energy_with_many_combinations(
        spark, from_date, to_date, False
    ).orderBy(F.rand())

    # Act
    actual_df = prepare_for_csv(
        df,
        True,
        requesting_actor_market_role,
    )

    # Assert
    assert actual_df.collect() == actual_df.orderBy(expected_order).collect()


def test_prepare_for_csv__when_datahub_admin__returns_dataframe_with_expected_ordering(
    spark: SparkSession,
) -> None:
    # Arrange
    from_date = datetime(2023, 3, 10, 22)
    to_date = datetime(2023, 3, 12, 22)

    expected_order = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.time,
    ]

    df = _create_energy_with_many_combinations(spark, from_date, to_date, True).orderBy(
        F.rand()
    )

    # Act
    actual_df = prepare_for_csv(
        df,
        True,
        MarketRole.DATAHUB_ADMINISTRATOR,
    )

    # Assert
    assert actual_df.collect() == actual_df.orderBy(expected_order).collect()
