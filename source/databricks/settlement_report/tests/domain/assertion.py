from pyspark.sql import SparkSession


def assert_files(
    path: str,
    actual_files: list[str],
    expected_columns: list[str],
    expected_file_names: list[str],
    spark: SparkSession,
):
    assert len(actual_files) == len(expected_file_names)
    for file_path in actual_files:
        df = spark.read.csv(f"{path}/{file_path}", header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert any(file_name in file_path for file_name in expected_file_names)
