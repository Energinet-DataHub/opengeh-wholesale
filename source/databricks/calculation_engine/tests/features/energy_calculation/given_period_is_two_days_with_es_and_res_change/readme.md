# Given period is two days with ES and RES change

The purpose is to test every resolution (from 1H to 15M), es and brp change for metering point types relevant for energy
results.

## Design considerations

- Input period is post May 2023 so that results are quarterly
- Metering point id's denote the type
    - Production metering points start with e.g. '18' for production metering points
    - Metering points with resolution 15M have a 15 in them
    - Nonprofiled metering points end with 1xx, flex with 2xx
    - Example - flex metering point with resolution 15M: '**17**000000**15**00000**2**01'

## Coverage
 - Change of balance responsible on an MP
 - Change of energy supplier on an MP
 - Change of resolution on an MP

