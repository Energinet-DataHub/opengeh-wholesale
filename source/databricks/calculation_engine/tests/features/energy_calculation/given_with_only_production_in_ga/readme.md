# Given only production MP in GA

The purpose of this test is the scenario where a grid area only has non-profiled consumption metering points,
specifically that grid loss calculation is correct.

## Design considerations

- Input period is post May 2023 so that results are quarterly
- Input data is mostly minimal standard scenario, but without consumption metering points.

## Coverage
 - Grid area only has Production MPs, but no consumption MPs, except for Grid Loss MP
