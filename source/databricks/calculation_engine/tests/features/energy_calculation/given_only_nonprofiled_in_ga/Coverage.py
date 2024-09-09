from test_coverage.all_test_cases import Cases

"""
## PURPOSE
The purpose of this test is the scenario where a grid area only has non-profiled consumption metering points,
specifically that grid loss calculation is correct.

## DESIGN CONSIDERATIONS
- Input period is post May 2023 so that results are quarterly

## CASES TESTED
"""
Cases.CalculationTests.UnusualGridAreaSetups.Grid_area_with_only_non_profiled_MP
Cases.CalculationTests.UnusualGridAreaSetups.Energy_Supplier_only_has_Grid_Loss_MP_or_System_Correction_MP
