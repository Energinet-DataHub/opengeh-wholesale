from test_coverage.all_test_cases import Tests

"""
## PURPOSE ##
The purpose of this test is the scenario where a grid area has no consumption metering points,
specifically that grid loss calculation is correct.

## DESIGN CONSIDERATIONS ## 
- Input period is post May 2023 so that results are quarterly
- Input data is mostly minimal standard scenario, but without consumption metering points.

## CASES TESTED ##
"""
Tests.CalculationTests.UnusualGridAreaSetups.Grid_area_with_only_production_MP
Tests.CalculationTests.UnusualGridAreaSetups.Energy_Supplier_only_has_Grid_Loss_MP_or_System_Correction_MP
