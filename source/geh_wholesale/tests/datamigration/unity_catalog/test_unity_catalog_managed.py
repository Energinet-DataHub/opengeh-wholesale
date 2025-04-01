"""
This file should be renamed and relocated when hive is removed (or earlier).
"""

import pytest
from pyspark.sql import SparkSession

from geh_wholesale.infrastructure import paths


@pytest.mark.parametrize(
    ("schema_name", "table_name"),
    [
        (
            paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
            paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
        ),
        (
            paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
            paths.WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
        ),
        (
            paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
            paths.WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
        ),
        (
            paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
            paths.WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
        ),
        (
            paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
            paths.WholesaleResultsInternalDatabase.EXCHANGE_PER_NEIGHBOR_TABLE_NAME,
        ),
        (
            paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
            paths.WholesaleResultsInternalDatabase.AMOUNTS_PER_CHARGE_TABLE_NAME,
        ),
        (
            paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
            paths.WholesaleResultsInternalDatabase.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
        ),
        (
            paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
            paths.WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.DATABASE_NAME,
            paths.WholesaleBasisDataInternalDatabase.METERING_POINT_PERIODS_TABLE_NAME,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.DATABASE_NAME,
            paths.WholesaleBasisDataInternalDatabase.TIME_SERIES_POINTS_TABLE_NAME,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.DATABASE_NAME,
            paths.WholesaleBasisDataInternalDatabase.CHARGE_LINK_PERIODS_TABLE_NAME,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.DATABASE_NAME,
            paths.WholesaleBasisDataInternalDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.DATABASE_NAME,
            paths.WholesaleBasisDataInternalDatabase.CHARGE_PRICE_POINTS_TABLE_NAME,
        ),
        (
            paths.WholesaleInternalDatabase.DATABASE_NAME,
            paths.WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME,
        ),
        (
            paths.WholesaleInternalDatabase.DATABASE_NAME,
            paths.WholesaleInternalDatabase.GRID_LOSS_METERING_POINT_IDS_TABLE_NAME,
        ),
    ],
)
def test__when_migrations_executed__created_table_is_managed(
    spark: SparkSession, migrations_executed: None, schema_name: str, table_name: str
) -> None:
    """
    It has been decided that all Delta Tables in the system should be managed, since it gives several benefits
    such enabling more Databricks features and ensuring that access rights are only managed by Unity Catalog
    """

    table_description = spark.sql(f"DESCRIBE EXTENDED {schema_name}.{table_name}")
    table_description.show()

    is_managed = any(
        prop["col_name"] == "Type" and prop["data_type"] == "MANAGED" for prop in table_description.collect()
    )

    assert is_managed
