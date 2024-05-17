# When Minimal Standard Scenario

The purpose of this test is the scenario where a grid area only has non-profiled consumption metering points, specifically that grid loss calculation is correct.

## Design considerations

- Input period is post May 2023 so that results are quarterly
- Input data is mostly minimal standard scenario, but without flex metering points

## Coverage

All metering point types relevant for energy calculations

- Exchange (energy in)
- Non-profiled consumption
- Flex consumption (grid loss)
- Production
- Production (system correction)
