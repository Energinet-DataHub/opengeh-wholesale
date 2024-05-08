# When Minimal Standard Scenario

The purpose of this test is the scenario that an exchange metering point is in a grid area, but is exchanging between
two other grid areas than the one it is in.

## Design considerations

- Input period is post May 2023 so that results are quarterly
- An E20 sending power into the grid area is included, otherwise we cannot calculate grid loss. But no readings are
  included for it in time series.
- Oracle Excel-sheet included (Oracle - exchange between 2 other ga.xlsx)

## Coverage

- E20 (energy in)
- E20 (two other GA)
- E17 (flex)
- E17 (nonprofiled)
- E17 (glmp)
- E18
- E18 (skmp)


