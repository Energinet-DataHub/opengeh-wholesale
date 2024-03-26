# Testing monthly tariff per grid area, charge owner and energy supplier with negative grid loss from hourly

```text
GIVEN grid loss and system correction metering points (positive and negative grid loss)
  AND one consumption metering point
  AND 5 hours for consumption metering point (5 rows)
WHEN calculating the monthly negative grid loss for February
THEN the aggregated amount is 37.849900 for negative grid loss (system correction)
```

```text
Negative grid loss is calculated like this:

(((Σ Exchange in - Σ Exchange out) + Σ Production) - (Σ Consumption non-profiled + Σ Consumption flex))) = grid loss per hour
grid loss per hour * hours per day * days per month = total grid loss

(((0 - 0) + 0) - (0 + 10)) = -10 * 5 = -50

tariff * total negative grid loss = amount
0.756998 * -50 = -37.849900
```
