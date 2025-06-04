from geh_wholesale.codelists import CalculationType
from geh_wholesale.codelists.calculation_type import is_wholesale_calculation_type


def test__is_wholesale_calculation_type__returns_expected():
    # Arrange
    expected_results = {
        CalculationType.BALANCE_FIXING: False,
        CalculationType.AGGREGATION: False,
        CalculationType.WHOLESALE_FIXING: True,
        CalculationType.FIRST_CORRECTION_SETTLEMENT: True,
        CalculationType.SECOND_CORRECTION_SETTLEMENT: True,
        CalculationType.THIRD_CORRECTION_SETTLEMENT: True,
    }

    for calculation_type in CalculationType:
        # Act
        result = is_wholesale_calculation_type(calculation_type)

        # Assert
        assert calculation_type in expected_results.keys()
        assert result == expected_results[calculation_type]
