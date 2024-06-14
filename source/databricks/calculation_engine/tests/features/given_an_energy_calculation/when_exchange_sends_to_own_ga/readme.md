# When Minimal Standard Scenario

The purpose is to test a scenario where there are exchange MPs where from and to grid area is the same.

## Design considerations

- Two variants are tested:
    - One is MP 200000000000000002 which is in another grid area (802) but sends from and to the grid area for this
      calculation (800).
    - One is MP 200000000000000004 which is in the grid area for this calculation (800) but sends to and from another
      grid area (804).
- Expected behaviour is that the measurements from the former are included in the calculation, but not from the latter.

## Coverage

All metering point types relevant for energy calculations

- Exchange (energy in)
- Exchange (energy out)
- Consumption (flex)
- Consumption (nonprofiled)
- Production
- Grid Loss MP
- System Correction MP

Each of the metering point types both have a metering point with resolution 15M and 1H.
