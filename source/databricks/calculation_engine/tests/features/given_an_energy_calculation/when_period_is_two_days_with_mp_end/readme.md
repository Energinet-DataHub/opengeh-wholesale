# When Minimal Standard Scenario

The purpose is to test that we can handle a metering point being discontinued.

## Design considerations

- Input period is two days with four of the metering points ending between the two days
- On day 2, a new exchange metering point is initiated

## Coverage

Metering point types:

- Exchange (energy in)
- Flex consumption
- Nonprofiled consumption
- Production
- Flex consumption - grid loss
- Production - system correction
