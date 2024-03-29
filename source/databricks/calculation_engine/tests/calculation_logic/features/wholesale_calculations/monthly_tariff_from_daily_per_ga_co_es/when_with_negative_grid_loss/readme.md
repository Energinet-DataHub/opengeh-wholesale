# Testing monthly tariff per grid area, charge owner and energy supplier with negative grid loss from daily

```text
GIVEN grid loss and system correction metering points (positive and negative grid loss)
  AND one consumption metering point
  AND one exchange metering point  
  AND one production metering point
  AND one monthly time series point for consumption (24 hours * 28 days = 672 rows)
  AND one monthly time series point for production  (24 hours * 28 days = 672 rows)
WHEN calculating the monthly negative grid loss for February
THEN the aggregated amount is 12015.58792 for positive grid loss (grid loss) 
THEN the aggregated amount is 3795.11568 for negative grid loss (system correction)
```

```text
Negative grid loss is calculated like this:

(((Σ Exchange in - Σ Exchange out) + Σ Production) - (Σ Consumption non-profiled + Σ Consumption flex))) = grid loss
(((0 - 0) + 90) - (0 + 75)) = 3.75 * 24 * 19 = 6840 
(((0 - 0) + 80) - (0 + 90)) = -2.5 * 24 * 9 = -2160 

tariff * positive grid loss = amount
1.756998 * 6840 = 12017.86632

tariff * negative grid loss = amount
1.756998 * -2160 = -3795.11568
```
