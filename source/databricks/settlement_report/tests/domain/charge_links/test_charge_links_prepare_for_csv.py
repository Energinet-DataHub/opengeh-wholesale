from datetime import datetime

import pytest
from pyspark.sql import SparkSession, functions as F, DataFrame

from settlement_report_job.domain.charge_links.prepare_for_csv import prepare_for_csv
from settlement_report_job.domain.csv_column_names import CsvColumnNames
from settlement_report_job.domain.market_role import MarketRole

from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    MeteringPointTypeDataProductValue,
    ChargeTypeDataProductValue,
)


def _create_charge_link_periods_with_many_combinations(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
) -> DataFrame:
    rows = []
    for grid_area_code in ["804", "805"]:
        for mp_type in [
            MeteringPointTypeDataProductValue.CONSUMPTION,
            MeteringPointTypeDataProductValue.PRODUCTION,
        ]:
            for mp_id in ["1", "2"]:
                for energy_supplier_id in ["1000000000000", "2000000000000"]:
                    rows.append(
                        {
                            DataProductColumnNames.metering_point_id: mp_id,
                            DataProductColumnNames.metering_point_type: mp_type.value,
                            DataProductColumnNames.charge_type: ChargeTypeDataProductValue.TARIFF.value,
                            DataProductColumnNames.charge_code: "41000",
                            DataProductColumnNames.charge_owner_id: "9876543210123",
                            DataProductColumnNames.quantity: 1,
                            DataProductColumnNames.from_date: from_date,
                            DataProductColumnNames.to_date: to_date,
                            DataProductColumnNames.grid_area_code: grid_area_code,
                            DataProductColumnNames.energy_supplier_id: energy_supplier_id,
                        }
                    )

    return spark.createDataFrame(rows)


EXPECTED_ORDERING_FOR_DATAHUB_ADMINISTRATOR_AND_SYSTEM_OPERATOR = [
    CsvColumnNames.energy_supplier_id,
    CsvColumnNames.metering_point_type,
    CsvColumnNames.metering_point_id,
    CsvColumnNames.charge_owner_id,
    CsvColumnNames.charge_code,
    CsvColumnNames.charge_link_from_date,
]

EXPECTED_ORDERING_FOR_ENERGY_SUPPLIER_AND_GRID_ACCESS_PROVIDER = [
    CsvColumnNames.metering_point_type,
    CsvColumnNames.metering_point_id,
    CsvColumnNames.charge_owner_id,
    CsvColumnNames.charge_code,
    CsvColumnNames.charge_link_from_date,
]


@pytest.mark.parametrize(
    "requesting_actor_market_role,expected_order",
    [
        (
            MarketRole.DATAHUB_ADMINISTRATOR,
            EXPECTED_ORDERING_FOR_DATAHUB_ADMINISTRATOR_AND_SYSTEM_OPERATOR,
        ),
        (
            MarketRole.SYSTEM_OPERATOR,
            EXPECTED_ORDERING_FOR_DATAHUB_ADMINISTRATOR_AND_SYSTEM_OPERATOR,
        ),
        (
            MarketRole.ENERGY_SUPPLIER,
            EXPECTED_ORDERING_FOR_ENERGY_SUPPLIER_AND_GRID_ACCESS_PROVIDER,
        ),
        (
            MarketRole.GRID_ACCESS_PROVIDER,
            EXPECTED_ORDERING_FOR_ENERGY_SUPPLIER_AND_GRID_ACCESS_PROVIDER,
        ),
    ],
)
def test_prepare_for_csv__when_daylight_saving_tim_transition__returns_dataframe_with_expected_ordering(
    spark: SparkSession,
    requesting_actor_market_role: MarketRole,
    expected_order: list[str],
) -> None:
    # Arrange
    from_date = datetime(2023, 3, 10, 22)
    to_date = datetime(2023, 3, 12, 22)
    df = _create_charge_link_periods_with_many_combinations(
        spark, from_date, to_date
    ).orderBy(F.rand())

    # Act
    actual_df = prepare_for_csv(
        charge_link_periods=df,
        requesting_actor_market_role=requesting_actor_market_role,
    )

    # Assert
    assert actual_df.collect() == actual_df.orderBy(expected_order).collect()
