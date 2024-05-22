# When minimal standard wholesale scenario

The purpose is to test that the wholesale specific views for settlement reports returns the expected data for the minimal standard wholesale scenario.

## Design considerations

- Verify that only succeeded calculations are included (those that are in the 'calculations' delta table)
- Verify that metering points that do not have a charge link is not part of the result

## Coverage

- There are two calculation IDs in charge link periods, but only one of them is in 'calculations'
- There are two metering points in 'metering_point_periods', but only one of them is in 'charge_link_periods'
