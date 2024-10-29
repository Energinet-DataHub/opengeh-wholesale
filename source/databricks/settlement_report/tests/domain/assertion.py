def assert_files(actual_files, expected_columns, expected_file_names, spark):
    assert len(actual_files) == len(expected_file_names)
    for file_path in actual_files:
        df = spark.read.csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert any(file_name in file_path for file_name in expected_file_names)
