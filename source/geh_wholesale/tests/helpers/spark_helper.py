from pyspark.sql import SparkSession


def reset_spark_catalog(spark: SparkSession) -> None:
    schemas = spark.catalog.listDatabases()
    for schema in schemas:
        if schema.name != "default":
            spark.sql(f"DROP DATABASE IF EXISTS {schema.name} CASCADE")
