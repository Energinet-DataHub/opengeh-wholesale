# When Minimal Standard Scenario

The purpose of this test is to check the edge case where a grid area only has GLMP, SKMP and no other metering points, except an E20 which is a neighbour grid area, sending energy into the grid area.

## Design considerations

- Period is one hour
- Input period is post May 2023 so that results are quarterly
- Since energy is going in to grid area, there should be grid loss on the GLMP metering point

## Coverage

- E20 (energy in - 1H)
- E17 (glmp - 15M)
- E18 (skmp - 15M)
