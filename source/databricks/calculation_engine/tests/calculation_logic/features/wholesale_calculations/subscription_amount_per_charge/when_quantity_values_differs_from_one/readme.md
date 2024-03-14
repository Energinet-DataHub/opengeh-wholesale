# Testing subscription amount for mixed quantity values

```
GIVEN two consumption metering points and one production metering point
  AND the consumption metering points have a charge link with quantity of 2, 4 pieces
  AND the production metering point has a link with quantity of 3 pieces  
  AND the subscription price is 28.282828 DKK
WHEN calculating subscription amount per charge for February
THEN the subscription price is 1.010101 DKK
  AND the subscription amount is 6.060606 DKK per day for the consumption metering points
  AND the subscription amount is 3.030303 DKK per day for the production metering points
```
