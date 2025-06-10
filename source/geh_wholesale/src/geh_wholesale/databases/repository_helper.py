from geh_common.testing.dataframes.assert_schemas import assert_contract
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import StructType


def read_table(
    spark: SparkSession,
    catalog_name: str,
    database_name: str,
    table_name: str,
    contract: StructType,
) -> DataFrame:
    name = f"{catalog_name}.{database_name}.{table_name}"
    df = spark.read.format("delta").table(name)

    # Assert that the schema of the data matches the defined contract
    assert_contract(df.schema, contract)

    # Select only the columns that are defined in the contract to avoid potential downstream issues
    df = df.select(contract.fieldNames())

    return df
