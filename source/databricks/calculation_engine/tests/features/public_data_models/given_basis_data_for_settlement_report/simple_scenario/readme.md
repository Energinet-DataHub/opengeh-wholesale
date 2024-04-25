# Simple scenario of settlement report views

```text
GIVEN two production metering point for energy supplier 8100000000108
  AND four time series points
WHEN updating tables metering_point_periods_v1 and time_series_points_v1
THEN the view metering_point_periods_v1 contains two rows
  AND quantities is aggregated to respectively one and three elements
  AND observation_day's are 2023-02-01 and 2023-03-01
```
