from test_coverage.all_test_cases import Cases

"""
## PURPOSE
The purpose of this test is to check the edge case where a grid area only has GLMP, SKMP and an exchange metering point.
I.e. no consumption or production metering points.

## DESIGN CONSIDERATIONS
- Period is one hour
- The exchange metering point is in a neighbor grid area
- Energy goes into the grid area, putting non-zero quantities on GLMP and zero quantities on SKMP
- Input period is post May 2023 so that results are quarterly
- Oracle Excel-sheet included (Oracle - only E20 and GLMP-SKMP.xlsx)

## CASES TESTED
"""
Cases.CalculationTests.UnusualGridAreaSetups.Grid_area_with_only_exchange_MP
Cases.CalculationTests.UnusualGridAreaSetups.Energy_Supplier_only_has_Grid_Loss_MP_or_System_Correction_MP
