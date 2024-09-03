from test_coverage.all_test_cases import Tests

"""
## PURPOSE ##
The purpose of this test is the scenario where a grid area only has non-profiled consumption metering points,
specifically that grid loss calculation is correct.

## DESIGN CONSIDERATIONS ## 
- Input period is post May 2023 so that results are quarterly
- Input data is mostly minimal standard scenario, but without flex metering points

## CASES TESTED ##
"""
Tests.CalculationTests.UnusualGridAreaSetups.Grid_area_only_has_nonprofiled_MPs_Production_MPs_but_no_Flex_MPs_except_for_Grid_Loss_MP
