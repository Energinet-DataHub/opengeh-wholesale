# When Minimal Standard Scenario

The purpose of this test is to check the edge case where a grid area only has GLMP, SKMP and an exchange metering point.
I.e. no consumption or production metering points.

## Design considerations

- Period is one hour
- The exchange metering point is in a neighbour grid area
- Energy goes into the grid area, putting non-zero quantities on GLMP and zero quantities on SKMP
- Input period is post May 2023 so that results are quarterly
- Oracle Excel-sheet included (Oracle - only E20 and GLMP-SKMP.xlsx)

## Coverage

- E20 (energy in - 1H)
- E17 (glmp - 15M)
- E18 (skmp - 15M)
