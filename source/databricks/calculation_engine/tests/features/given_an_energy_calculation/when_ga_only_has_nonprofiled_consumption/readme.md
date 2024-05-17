# When Minimal Standard Scenario

The purpose of this test is to test every energy result calculation (along with basis data) with the smallest meaningful
time period (one hour).

## Design considerations

- Input period is post May 2023 so that results are quarterly
- Input quantities have deliberately set to produce different quarterly results due to rounding for at least one of each
  metering point type
- Input quantities have been set so production, flex, and nonprofiled do not produce the same result.
- Energy Supplier and Balance Responsible id's have set so that ga_es, ga_brp and ga_brp_es do not produce the same
  result/rows.
- Oracle Excel-sheet included (Oracle - minimal standard calculation.xlsx)
- Metering point id's denote the type
    - Production metering points start with e.g. '18' for production metering points
    - Metering points with resolution 15M have a 15 in them
    - Nonprofiled metering points end with 1xx, flex with 2xx
    - Example - flex metering point with resolution 15M: '**17**000000**15**00000**2**01'

## Coverage

All metering point types relevant for energy calculations

- E20 (energy in)
- E20 (energy out)
- E17 (flex)
- E17 (nonprofiled)
- E17 (glmp)
- E18
- E18 (skmp)

Each of the metering point types both have a metering point with resolution 15M and 1H.
