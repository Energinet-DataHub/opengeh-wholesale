# When Minimal Standard Scenario

The purpose of this test is the scenario where a grid area only has non-profiled consumption metering points, specifically that grid loss calculation is correct.

## Design considerations

- Input period is post May 2023 so that results are quarterly
- Input data is mostly minimal standard scenario, but without consumption metering points.

## Coverage

All metering point types relevant for energy calculations

- Exchange (energy in)
- Exchange (energy out)
- Flex consumption (grid loss)
- Production (system correction)
- Production
