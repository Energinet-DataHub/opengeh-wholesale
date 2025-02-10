import pytest
from geh_common.testing.etl import get_then_names, TestCases
from geh_common.testing.dataframes import (
    assert_dataframes_and_schemas,
    AssertDataframesConfiguration,
)


@pytest.mark.parametrize("name", get_then_names())
def test__equals_expected(
    test_cases: TestCases,
    name: str,
    assert_dataframes_configuration: AssertDataframesConfiguration,
) -> None:
    test_case = test_cases[name]

    assert_dataframes_and_schemas(
        actual=test_case.actual,
        expected=test_case.expected,
        configuration=assert_dataframes_configuration,
    )
