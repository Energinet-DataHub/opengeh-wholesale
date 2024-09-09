from test_coverage.all_test_cases import Tests

"""
## PURPOSE
The purpose is to check the scenario where a price element is active and has active charge link, but there are no active
prices in the calculation period.

## CASES TESTED
"""
Tests.CalculationTests.PriceElementsAndMPPeriods.Price_element_from_date_inside_calculation_period
Tests.CalculationTests.PriceElementsAndMPPeriods.Price_element_to_date_inside_calculation_period
