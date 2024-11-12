from pyspark.sql import SparkSession


def assert_file_names_and_columns(
    path: str,
    actual_files: list[str],
    expected_columns: list[str],
    expected_file_names: list[str],
    spark: SparkSession,
):
    assert set(actual_files) == set(expected_file_names)
    for file_name in actual_files:
        df = spark.read.csv(f"{path}/{file_name}", header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
