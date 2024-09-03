from test_coverage.all_test_cases import Tests

"""
## PURPOSE ##
The purpose is to test that we can handle a metering point being discontinued.

## DESIGN CONSIDERATIONS ## 
- Input period is two days with four of the metering points ending between the two days
- On day 2, a new exchange metering point is initiated

## CASES TESTED ##
"""
Tests.CalculationTests.MeteringPointMasterDataUpdates.MP_is_shut_down_in_calculation_period
