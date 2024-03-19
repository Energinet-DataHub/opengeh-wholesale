# Calculate positive grid loss

```text
GIVEN grid loss and system correction metering points (positive and negative grid loss)
  AND one consumption metering point
  AND one exchange metering point  
  AND one production metering point
WHEN calculating positive grid loss for 1st of February
THEN the calculated positive grid loss is 3.750 kWh per quarter for the period
THEN there are 96 rows in the result
```

```text
The grid loss is calculated like this:
(Sum E18 + (Sum E20 in - Sum E20 out) - (Sum E17 non-profiled + Sum E17 flex)) / 4 = grid loss
(10 + (10 - 0) - (0 + 5)) / 4 = 3.750

When grid loss > 0 then grid loss is positive

The number of rows is calculated by multiplying the number quarters by the number of hours times number of days.
4 * 24 * 1 = 96
```
