# Given period is two days with settlement change

The purpose is to test that an consumption metering point changes settlement method

## Design considerations

- Input period is post May 2023 so that results are quarterly
- Input period is two days with settlement method change between the two days
- Included is a flex metering point changing to non-profiled and a non-profiled changing to flex. Both metering points
  also change resolution from 1H to 15M on day two.
- There is positive grid loss on day one and negative grid loss on day two. Production results are included to check
  negative grid loss.

## Coverage

- Change of settlement method on an MP
- Change of resolution on an MP
