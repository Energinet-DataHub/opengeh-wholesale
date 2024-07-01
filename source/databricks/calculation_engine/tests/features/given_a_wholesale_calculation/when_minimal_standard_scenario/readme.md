# When Minimal Standard Scenario

The purpose of this test is the scenario that an exchange metering point is in a grid area, but is exchanging between
two other grid areas than the one it is in.

## Design considerations

- fee på parent, starter midt i måned
- fee på child

## Coverage

3 fees, all start date before calculation period

charge masterdata
 - from-to covers whole calc. period
 
charge link periods
 - quantity = 1
 - quantity > 1

 
fee_per_ga_co_es

Aggregation
	charge time (day)
	charge-key
	MP type

Check that identical aggregations are correctly summed and different aggregations have separate rows

monthly_fee_per_ga_co_es

Aggregation
	charge time (month)
	charge-key
Check that identical aggregations are correctly summed and different aggregations have separate rows



