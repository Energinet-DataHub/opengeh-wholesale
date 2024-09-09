from test_coverage.all_test_cases import Tests

"""
## PURPOSE
The purpose is to test a calculation on a period before the transition to quarterly results.

## DESIGN CONSIDERATIONS
- Period is before the calculation result resolution change
- Input data has two of each production MP, a consumption MP of both types of settlement methods and exchange MP - one with resolution 15M and one with resolution 1H  
- Time series for each MP

## CASES TESTED
"""
Tests.CalculationTests.Calculation_results_are_hourly_when_calculation_period_is_before_result_resolution_change
