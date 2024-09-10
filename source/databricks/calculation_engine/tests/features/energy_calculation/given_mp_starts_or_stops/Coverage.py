from test_coverage.all_test_cases import Cases

"""
## PURPOSE
The purpose is to test that we can handle a metering point being discontinued.

## DESIGN CONSIDERATIONS
- Input period is two days with four of the metering points ending between the two days
- On day 2, a new exchange metering point is initiated

## CASES TESTED
"""
Cases.CalculationTests.MeteringPointMasterDataUpdates.MP_is_shut_down_in_calculation_period
Cases.CalculationTests.MeteringPointMasterDataUpdates.MP_is_starts_up_in_calculation_period
Cases.CalculationTests.UnusualGridAreaSetups.Energy_Supplier_only_has_Grid_Loss_MP_or_System_Correction_MP
