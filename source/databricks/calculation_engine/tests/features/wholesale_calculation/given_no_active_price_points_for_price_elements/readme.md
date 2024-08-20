# When There Are No Active Price Points for Price Elements

The purpose is to check the scenario where a price element is active and has active charge link, but there are no active
prices in the calculation period.

## Coverage

- All types of wholesale calculations - fee, subscription, hourly and daily tariffs - are included.
- Period is two days.
- A small set of energy time series is included in order to test tariffs.
- There are elements of each wholesale calculation included, but only one of them has active price points in the calculation period.
- Verifying that 'price' and 'amount' is null, if there is no active price element.
