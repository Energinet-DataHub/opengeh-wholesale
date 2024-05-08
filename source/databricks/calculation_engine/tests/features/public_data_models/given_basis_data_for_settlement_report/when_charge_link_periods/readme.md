# When charge link periods basis data is available

The purpose of this test is to test the view for settlement reports that is used to get charge link periods basis data.

## Design considerations

- Verify that only only succeeded calculations are included (those that are in the 'calculation' delta table)
- Verify that metering points that do not have a charge link is not part of the result

## Coverage

- There are two calculation IDs in charge link periods, but only one of them is in 'calculations'
- There are two metering points in 'metering_point_periods', but only one of them is in 'charge_link_periods'
