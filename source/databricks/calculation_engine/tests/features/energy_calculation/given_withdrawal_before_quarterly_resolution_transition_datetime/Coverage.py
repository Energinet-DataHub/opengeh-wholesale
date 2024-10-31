from coverage.all_test_cases import Cases

"""
## PURPOSE
The purpose is to test withdrawal on a period before the transition to quarterly results.

## DESIGN CONSIDERATIONS
- Period is before the calculation result resolution change
- Input data has two of each production MP, a consumption MP of both types of settlement methods and exchange MP - one with resolution 15M and one with resolution 1H
- Time series for each MP

## CASES TESTED
"""
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_new_calculation_period_before_quarterly_transition
