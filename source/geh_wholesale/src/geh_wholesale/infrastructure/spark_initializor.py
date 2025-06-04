from pyspark import SparkConf
from pyspark.sql.session import SparkSession


def initialize_spark() -> SparkSession:
    # Set spark config with the session timezone so that datetimes are displayed consistently (in UTC)
    spark_conf = (
        SparkConf(loadDefaults=True)
        .set("spark.sql.session.timeZone", "UTC")
        .set("spark.databricks.io.cache.enabled", "True")
    )
    return SparkSession.builder.config(conf=spark_conf).getOrCreate()
