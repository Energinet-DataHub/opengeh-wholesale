from test_coverage.all_test_cases import Tests

"""
## PURPOSE ##
The purpose of this test is to check the edge case where a grid area only has GLMP, SKMP and an exchange metering point.
I.e. no consumption or production metering points.

## DESIGN CONSIDERATIONS ## 
- Period is one hour
- The exchange metering point is in a neighbor grid area
- Energy goes into the grid area, putting non-zero quantities on GLMP and zero quantities on SKMP
- Input period is post May 2023 so that results are quarterly
- Oracle Excel-sheet included (Oracle - only E20 and GLMP-SKMP.xlsx)

## CASES TESTED ##
"""
Tests.CalculationTests.UnusualGridAreaSetups.Grid_area_with_only_Exchange_MP_just_one_direction
Tests.CalculationTests.UnusualGridAreaSetups.Grid_area_with_only_Exchange_MP_and_Grid_Loss_MP_and_System_Correction_MP
Tests.CalculationTests.UnusualGridAreaSetups.Grid_with_E20_in_neighbour_grid_area_and_Grid_Loss_MP_and_System_Correction_MP
Tests.CalculationTests.UnusualGridAreaSetups.Energy_Supplier_only_has_Grid_Loss_MP_or_System_Correction_MP
Tests.CalculationTests.ExchangeCases.Exchange_MP_where_exchange_is_between_two_other_grid_areas_than_the_one_it_is_in
