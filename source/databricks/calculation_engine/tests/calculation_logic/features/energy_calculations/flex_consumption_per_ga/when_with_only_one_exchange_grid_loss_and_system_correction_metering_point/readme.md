# Calculate flex consumption per grid area and energy supplier with only one exchange, grid loss and system correction metering point

```text
GIVEN one exchange metering point
    And one grid loss metering point
    AND one system correction metering point
    AND time series on the exchange MP is 75 kWh per hour
WHEN calculating total flex_consumption per grid area
THEN flex_consumption per grid area is 75/4 = 18.75
THEN there are four rows
```
