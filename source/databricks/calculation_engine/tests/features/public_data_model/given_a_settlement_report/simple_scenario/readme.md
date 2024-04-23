# Simple test of settlement report views

```text
GIVEN a production metering point for energy supplier 8100000000108
  AND three time series points
WHEN updating tables metering_point_periods_v1 and time_series_points_v1
THEN the view metering_point_periods_v1 contains 1 row
  AND quantities is aggregated to X
  AND observation_day is Y
```
