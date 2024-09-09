from test_coverage.all_test_cases import Cases

"""
## PURPOSE
The purpose of this test is the scenario that an exchange metering point is in a grid area, but is exchanging between two other grid areas than the one it is in.

## DESIGN CONSIDERATIONS
An E20 sending power into the grid area is included, otherwise we cannot calculate grid loss. But no readings are included for it in time series.

## CASES TESTED
"""
Cases.CalculationTests.ExchangeCases.Exchange_between_two_ga_where_exchange_MP_is_in_neither_ga
