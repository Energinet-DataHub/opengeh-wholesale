# When Minimal Standard Scenario

The purpose of this test is to test views can handle wintertime change.

## Design considerations

- The period in input basis data is three days - one day before and after the wintertime change day (October 29th 2023)
- Only included the necessary MPs for a calculation to run (1 exchange, Grid Loss/System Correction MP). 
- The purpose is to check whether the arrays for one day in metering_point_time_series:
    - include cut off at the right time (22.45 rather than 21.45)
    - Do include readings from 21.00 to 22.00 on the 29th (have extra readings that day)

## Coverage

- Exchange (energy in)
- Consumption - Flex - Grid Loss MP
- Production - System Correction MP
