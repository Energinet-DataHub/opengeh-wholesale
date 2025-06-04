from tests.helpers.spark_sql_migration_helper import MigrationsExecution


class MigrationsConfiguration:
    def __init__(self, configuration: dict):
        configuration.setdefault("execute", MigrationsExecution.ALL.name)
        self.execute = MigrationsExecution[configuration["execute"]]


class FeatureTestsConfiguration:
    def __init__(self, configuration: dict):
        configuration.setdefault("show_actual_and_expected", False)
        configuration.setdefault("show_columns_when_actual_and_expected_are_equal", False)
        configuration.setdefault("show_actual_and_expected_count", False)
        configuration.setdefault("assert_no_duplicate_rows", False)
        self.show_actual_and_expected = configuration["show_actual_and_expected"]
        self.show_columns_when_actual_and_expected_are_equal = configuration[
            "show_columns_when_actual_and_expected_are_equal"
        ]
        self.show_actual_and_expected_count = configuration["show_actual_and_expected_count"]
        self.assert_no_duplicate_rows = configuration["assert_no_duplicate_rows"]


class TestSessionConfiguration:
    # Pytest test classes will fire a warning if it has a constructor (__init__).
    # To avoid a class being treated as a test class set the attribute __test__  to False.
    __test__ = False

    def __init__(self, configuration: dict):
        configuration.setdefault("migrations", {})
        configuration.setdefault("feature_tests", {})
        self.migrations = MigrationsConfiguration(configuration["migrations"])
        self.feature_tests = FeatureTestsConfiguration(configuration["feature_tests"])
