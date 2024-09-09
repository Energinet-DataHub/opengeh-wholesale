from test_coverage.all_test_cases import Tests

"""
## PURPOSE ##
The purpose of this test is to test every energy result calculation (along with basis data) with the smallest meaningful
time period (one hour).

## DESIGN CONSIDERATIONS ## 
- Input quantities have deliberately set to produce different quarterly results due to rounding for at least one of each
  metering point type
- Input quantities have been set so production, flex, and nonprofiled do not produce the same result.
- Input data has MPs not included in calculation
- Energy Supplier and Balance Responsible id's have set so that ga_es, ga_brp and ga_brp_es do not produce the same
  result/rows.
- Oracle Excel-sheet included (Oracle - minimal standard calculation.xlsx)
- Metering point id's denote the type
    - Production metering points start with e.g. '18' for production metering points
    - Metering points with resolution 15M have a 15 in them
    - Nonprofiled metering points end with 1xx, flex with 2xx
    - Example - flex metering point with resolution 15M: '**17**000000**15**00000**2**01'

## CASES TESTED ##
"""
Tests.CalculationTests.Typical_energy_scenario
Tests.CalculationTests.Calculation_input_data_includes_other_ga
