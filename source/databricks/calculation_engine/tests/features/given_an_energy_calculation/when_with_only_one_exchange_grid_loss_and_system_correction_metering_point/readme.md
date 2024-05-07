# When Minimal Standard Scenario

The purpose of this test is to check the edge case where a grid area only has GLMP, SKMP and an exchange metering point.
I.e. no consumption or production metering points.

## Design considerations

- Period is two hours:
    - In the first hour, there is positive gridloss, putting non-zero quantities on GLMP
    - In the second hour, there is negative gridloss, putting non-zero quantities on SKMP
- Input period is post May 2023 so that results are quarterly
- The two exchange metering points that sends energy into the grid area are in a neighbour grid area

## Coverage

- E20 (energy in - 1H)
- E20 (energy in - 15M)
- E20 (energy in - 1H)
- E20 (energy out - 15M)
- E17 (glmp - 15M)
- E18 (skmp - 15M)
