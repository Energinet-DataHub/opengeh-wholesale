from geh_common.testing.dataframes.assert_table import assert_table_properties


def test_table_properties(spark, migrations_executed):
    """Test that the properties of the tables follow the general requirements for tables managed using Spark/Databricks."""
    assert_table_properties(
        spark=spark,
        excluded_tables=["executed_migrations"],
    )
