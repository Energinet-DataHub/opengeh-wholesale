# Calculate positive grid loss

```text
GIVEN grid loss and system correction metering points (positive and negative grid loss)
  AND one consumption metering point
  AND one exchange metering point  
  AND one production metering point
  AND one hourly time series point for each metering point
WHEN calculating positive grid loss for 1st of February from 12pm to 1pm
THEN the calculated positive grid loss is 3.750 kWh per quarter for the 1 hour period 
THEN there are 4 rows in the result
```

```text
The grid loss is calculated like this:
(((Σ Exchange in - Σ Exchange out) + Σ Production) - (Σ Consumption non-profiled + Σ Consumption flex)) / 4 = grid loss
(((10 - 0) + 10) - (0 + 5))) / 4 = 3.750

When grid loss > 0 then grid loss is positive

The number of rows is calculated by multiplying the number quarters by the number of hours times number of days.
4 * 1 * 1 = 4
```
