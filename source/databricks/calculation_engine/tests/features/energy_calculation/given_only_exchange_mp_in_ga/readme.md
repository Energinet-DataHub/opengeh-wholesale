# Given only Exchange, GLMP and SKMP in GA

The purpose of this test is to check the edge case where a grid area only has GLMP, SKMP and an exchange metering point.
I.e. no consumption or production metering points.

## Design considerations

- Period is one hour
- The exchange metering point is in a neighbor grid area
- Energy goes into the grid area, putting non-zero quantities on GLMP and zero quantities on SKMP
- Input period is post May 2023 so that results are quarterly
- Oracle Excel-sheet included (Oracle - only E20 and GLMP-SKMP.xlsx)

## Coverage

- Grid area with only Exchange MP (just one direction)
- Grid area with only Exchange MP and Grid Loss MP and System Correction MP
- Grid with E20 in neighbour grid area and Grid Loss MP and System Correction MP
- Energy Supplier only has Grid Loss MP or System Correction MP
- Exchange MP where exchange is between two other grid areas than the one it is in
