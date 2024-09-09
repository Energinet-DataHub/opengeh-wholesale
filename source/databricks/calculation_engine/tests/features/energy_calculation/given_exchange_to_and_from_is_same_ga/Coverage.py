from test_coverage.all_test_cases import Cases

"""
## PURPOSE
The purpose is to test a scenario where there are exchange MPs where from and to grid area is the same.

## DESIGN CONSIDERATIONS
- Two variants are tested:
    - One is MP 200000000000000002 which is in another grid area (802) but sends from and to the grid area for this
      calculation (800).
    - One is MP 200000000000000004 which is in the grid area for this calculation (800) but sends to and from another
      grid area (804).
- Expected behaviour is that the measurements from the former are included in the calculation, but not from the latter.

## CASES TESTED
"""
Cases.CalculationTests.ExchangeCases.Exchange_MP_where_from_and_to_is_the_same_grid_area
