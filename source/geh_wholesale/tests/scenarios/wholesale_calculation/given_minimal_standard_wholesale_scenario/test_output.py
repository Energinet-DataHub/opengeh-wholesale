import pytest
from geh_common.testing.dataframes import (
    AssertDataframesConfiguration,
    assert_dataframes_and_schemas,
)
from geh_common.testing.scenario_testing import TestCase, get_then_names
from pyspark.sql import SparkSession


@pytest.mark.parametrize("name", get_then_names())
def test__equals_expected(
    test_cases: TestCase,
    name: str,
    spark: SparkSession,
    assert_dataframes_configuration: AssertDataframesConfiguration,
) -> None:
    spark.catalog.clearCache()

    test_case = test_cases[name]
    assert_dataframes_and_schemas(
        actual=test_case.actual,
        expected=test_case.expected,
        configuration=assert_dataframes_configuration,
    )
