# Testing monthly tariff per grid area, charge owner and energy supplier with negative grid loss from hourly

```text
GIVEN grid loss and system correction metering points (positive and negative grid loss)
  AND one consumption metering point
  AND one exchange metering point  
  AND one production metering point
  AND one monthly time series point for consumption metering point~~~~ (24 hours * 28 days = 672 rows)
WHEN calculating the monthly negative grid loss for February
THEN the aggregated amount is 5087.02656 for negative grid loss (system correction)
```

```text
Negative grid loss is calculated like this:

(((Σ Exchange in - Σ Exchange out) + Σ Production) - (Σ Consumption non-profiled + Σ Consumption flex))) = grid loss
(((0 - 0) + 0) - (0 + 10)) = -10 * 24 * 28 = -6720

tariff * grid loss = amount
0.756998 * -6720 = -5087.02656
```
