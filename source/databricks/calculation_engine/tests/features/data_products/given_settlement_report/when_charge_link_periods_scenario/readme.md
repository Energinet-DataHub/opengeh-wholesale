# When charge link periods scenario

The purpose is to test that the settlement reports data product view charge_link_periods returns the expected data.

## Design considerations

- Verify that various combinations of charge_link_periods and metering point periods produce the correct view data.

## Coverage

- There are 14 test cases involving 'metering_point_periods' and 'charge_link_periods'.
- See test_get_charge_link_metering_point_periods.py for details.
