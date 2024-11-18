from pyspark.sql import SparkSession, functions as F

from settlement_report_job.domain.utils.map_from_dict import map_from_dict


def test_map_from_dict__when_applied_to_new_col__returns_df_with_new_col(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame([("a", 1), ("b", 2), ("c", 3)], ["key", "value"])

    # Act
    mapper = map_from_dict({"a": "another_a"})
    actual = df.select("*", mapper[F.col("key")].alias("new_key"))

    # Assert
    expected = spark.createDataFrame(
        [("a", 1, "another_a"), ("b", 2, None), ("c", 3, None)],
        ["key", "value", "new_key"],
    )
    assert actual.collect() == expected.collect()


def test_map_from_dict__when_applied_as_overwrite__returns_df_with_overwritten_column(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame([("a", 1), ("b", 2), ("c", 3)], ["key", "value"])

    # Act
    mapper = map_from_dict({"a": "another_a"})
    actual = df.select(mapper[F.col("key")].alias("key"), "value")

    # Assert
    expected = spark.createDataFrame(
        [
            ("another_a", 1),
            (None, 2),
            (None, 3),
        ],
        ["key", "value"],
    )
    assert actual.collect() == expected.collect()
