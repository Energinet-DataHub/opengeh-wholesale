"""
This file should be renamed and relocated when hive is removed (or earlier).
"""

from pyspark.sql import SparkSession

from package.infrastructure import paths


def test__when_uc_migrations_executed__energy_results_table_exists(
    spark: SparkSession, migrations_executed: None
) -> None:
    """
    Test that the UC energy results table exists after the UC migrations have been executed.
    Note that there is no actual Unity Catalog when running tests, so the table is created in the default catalog.
    """
    assert spark.catalog.tableExists(
        f"{paths.WholesaleResultsInternalDatabase.DATABASE_NAME}.{paths.WholesaleResultsInternalDatabase.ENERGY_RESULT_TABLE_NAME}"
    )
