# Given exchange between two other GA

The purpose of this test is the scenario that an exchange metering point is in a grid area, but is exchanging between
two other grid areas than the one it is in.

## Design considerations

- An E20 sending power into the grid area is included, otherwise we cannot calculate grid loss. But no readings are
  included for it in time series.

## Coverage
-  Exchange between to grid areas where exchange MP is in neither grid area of the exchange
