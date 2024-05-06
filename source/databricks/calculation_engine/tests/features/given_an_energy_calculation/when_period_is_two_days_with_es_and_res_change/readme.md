# When Minimal Standard Scenario

The purpose is to test every resolution (from 1H to 15M), es and brp change for metering point types relevant for energy results.

## Design considerations

- Input period is post May 2023 so that results are quarterly
- Metering point id's denote the type
    - Production metering points start with e.g. '18' for production metering points
    - Metering points with resolution 15M have a 15 in them
    - Nonprofiled metering points end with 1xx, flex with 2xx
    - Example - flex metering point with resolution 15M: '**17**000000**15**00000**2**01'

## Coverage

- E20 (energy in)
- E17 (flex)
- E17 (nonprofiled)
- E17 (glmp)
- E18
- E18 (skmp)
