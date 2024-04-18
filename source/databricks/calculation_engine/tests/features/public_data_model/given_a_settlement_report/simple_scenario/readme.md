# Simple test of settlement report views

```text
GIVEN a production metering point for energy supplier 8100000000108
  AND a single time series point
WHEN updating tables metering_point_periods and time_series_points
THEN thw view metering_point_periods contain 1 row
  AND quantities is aggregated to X
  AND observation_day is Y
```
