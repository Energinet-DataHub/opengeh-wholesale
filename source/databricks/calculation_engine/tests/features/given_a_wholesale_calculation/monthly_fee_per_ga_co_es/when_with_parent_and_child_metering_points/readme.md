# Fee

```text
GIVEN a flex_consumption metering point for energy supplier 8100000000108 with two fee-1 links
  AND another flex_consumption metering points for energy supplier 8100000000100 with 10 fee-1 links
  AND a child consumption_from_grid with one fee-1 link
  AND the fee-1 price is 70 DDK
WHEN calculating fee
THEN there are 2 rows in the result
  AND the amount for fee for energy supplier 8100000000100 is 700 DDK
  AND the amount for fee for energy supplier 8100000000108 is 210 DDK
```
