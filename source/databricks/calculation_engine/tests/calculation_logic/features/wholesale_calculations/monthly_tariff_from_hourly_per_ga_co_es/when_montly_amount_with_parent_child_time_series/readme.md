# Testing monthly amount for hourly tariff with child metering point

```text
GIVEN two metering points (positive and negative grid loss)
  AND two consumption metering points  
  AND the charge link starts on February 27th
  AND the subscription price is 28.282828 DKK
WHEN calculating subscription amount per charge for February
THEN there is only result rows for 27th and 28th of february
  AND the subscription amount is 1.010101 DKK
```