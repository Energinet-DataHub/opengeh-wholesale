# Testing monthly amount for hourly tariff with parent and child metering points

```text
GIVEN two grid loss metering points (positive and negative grid loss)
  AND one parent metering point (consumption)  
  AND one child metering point (consumption from grid)
  AND the charge link quantity of 10 for child metering point starting on February 2nd
  AND the charge link quantity of 20 for for parent metering point starting February 2nd
  AND the charge price is 0.756998 DKK per hour
WHEN calculating monthly amount for hourly tariff for February
THEN the amount is 15261.07968 DKK
```

```text
The amount calculated like this:
charge price * quantity * 24 hour * days in month = amount for metering point
Child metering point: 0.756998 * 10 * 24 * 28 = 5087.02656
Parent metering point: 0.756998 * 20 * 24 * 28 = 10174.05312
5087.02656 + 10174.05312 = 15261.07968
```
